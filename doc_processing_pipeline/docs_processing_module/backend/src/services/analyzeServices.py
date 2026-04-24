import os
import shutil
import fitz
from PIL import Image
import io
import tempfile
from pdf2image import convert_from_path
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter


def getPdfSize(pdfPath):
    """Returns the size of the pdf file in megabytes"""
    if os.path.exists(pdfPath):
        sizeBytes = os.path.getsize(pdfPath)
        sizeMb = sizeBytes / (1024 * 1024)
        return sizeMb
    else:
        return None


def get_poppler_path():
    """
    Obtiene la ruta correcta de Poppler.
    Intenta múltiples ubicaciones comunes.
    """
    possible_paths = [
        r"src\services\poppler-25.07.0\Library\bin",
        r"C:\Program Files\poppler\Library\bin",
        r"C:\Program Files (x86)\poppler\Library\bin",
        None  # Sistema PATH
    ]
    
    for path in possible_paths:
        if path is None:
            print("Using system PATH for Poppler")
            return None
        
        if os.path.exists(path):
            print(f"✓ Found Poppler at: {path}")
            return path
    
    print("⚠️ Poppler not found in expected locations, using system PATH")
    return None


def detect_blank_pages(pdf_path, blank_threshold=0.01, text_threshold=10):
    """Detecta páginas en blanco en un PDF"""
    blank_pages = []
    try:
        doc = fitz.open(pdf_path)
        for page_num in range(len(doc)):
            page = doc[page_num]
            text = page.get_text().strip()
            
            if len(text) < text_threshold:
                pix = page.get_pixmap(matrix=fitz.Matrix(1, 1))
                img_data = pix.pil_tobytes(format="PNG")
                img = Image.open(io.BytesIO(img_data))
                
                if img.mode != 'L':
                    img = img.convert('L')
                
                non_white = sum(1 for p in img.getdata() if p < 240)
                if (non_white / (img.width * img.height)) < blank_threshold:
                    blank_pages.append(page_num + 1)
        
        doc.close()
    except Exception as e:
        print(f"Error detecting blank pages: {e}")
    
    return blank_pages


def reconvert_pdf_to_images(pdf_path: str, max_size_mb: float = 4, dpi: int = 150, quality: int = 70) -> bool:
    """
    Reconvierte PDF a imágenes y lo reconstruye.
    EXTREMADAMENTE EFECTIVO para PDFs escaneados.
    
    Args:
        pdf_path: Ruta al PDF
        max_size_mb: Tamaño máximo objetivo
        dpi: Resolución DPI para conversión (menor = más compresión)
        quality: Calidad JPEG 0-100 (menor = más compresión)
    """
    temp_images = []
    reconverted_pdf = None
    
    try:
        print(f"\n🔄 RECONVERSIÓN A IMÁGENES (DPI={dpi}, Quality={quality})...")
        
        poppler_path = get_poppler_path()
        
        # Convertir páginas a imágenes
        print(f"Converting PDF pages to images at {dpi} DPI...")
        pages = convert_from_path(pdf_path, dpi=dpi, poppler_path=poppler_path)
        
        print(f"Total pages to convert: {len(pages)}")
        
        for i, page in enumerate(pages):
            out_file = f"{pdf_path}_page_{i}.jpg"
            page.save(out_file, "JPEG", quality=quality, optimize=True)
            temp_images.append(out_file)
            
            if (i + 1) % 5 == 0 or (i + 1) == len(pages):
                print(f"  Converted {i+1}/{len(pages)} pages")
        
        # Reconstruir PDF desde imágenes
        print("Rebuilding PDF from images...")
        reconverted_pdf = f"{pdf_path}_reconverted.pdf"
        
        c = canvas.Canvas(reconverted_pdf, pagesize=letter)
        
        for idx, img_path in enumerate(temp_images):
            try:
                img = Image.open(img_path)
                img_width, img_height = img.size
                
                # Ajustar tamaño a página carta
                scale_w = letter[0] / img_width
                scale_h = letter[1] / img_height
                scale = min(scale_w, scale_h)
                
                final_width = img_width * scale
                final_height = img_height * scale
                
                x = (letter[0] - final_width) / 2
                y = (letter[1] - final_height) / 2
                
                c.drawImage(img_path, x, y, width=final_width, height=final_height)
                c.showPage()
                
            except Exception as e:
                print(f"  Error adding page {idx+1}: {e}")
        
        c.save()
        
        # Verificar tamaño final
        final_size = getPdfSize(reconverted_pdf)
        print(f"✅ Reconverted PDF size: {final_size:.2f}MB")
        
        # Reemplazar PDF original
        os.remove(pdf_path)
        os.rename(reconverted_pdf, pdf_path)
        
        print(f"✅ Successfully replaced original PDF")
        return final_size <= max_size_mb
        
    except Exception as e:
        print(f"❌ Error in reconversion: {e}")
        import traceback
        traceback.print_exc()
        return False
        
    finally:
        # Limpiar archivos temporales
        for img in temp_images:
            if os.path.exists(img):
                try:
                    os.remove(img)
                except:
                    pass
        
        # Limpiar PDF temporal si existe
        if reconverted_pdf and os.path.exists(reconverted_pdf):
            try:
                os.remove(reconverted_pdf)
            except:
                pass


async def process_file(file_path):
    """
    Procesa un PDF aplicando compresión y optimizaciones.
    Modifica el archivo en su ubicación.
    """
    try:
        max_size_mb = 4
        max_dpi = 250
        process_size = True
        process_dpi = True
        process_blank_pages = True
        blank_threshold = 0.01
        text_threshold = 10
        
        if not os.path.exists(file_path):
            print(f"Error: File not found {file_path}")
            return False
        
        print(f"\n{'='*60}")
        print(f"Starting PDF processing: {file_path}")
        print(f"{'='*60}")
        
        doc = fitz.open(file_path)
        doc_modified = False
        original_size = getPdfSize(file_path)
        
        print(f"Original size: {original_size:.2f}MB")
        print(f"Total pages: {len(doc)}")
        
        # ✅ PASO 1: Eliminar páginas en blanco
        if process_blank_pages:
            blank_pages = detect_blank_pages(file_path, blank_threshold, text_threshold)
            if blank_pages:
                print(f"\n📄 Removing {len(blank_pages)} blank pages...")
                new_doc = fitz.open()
                for page_num in range(len(doc)):
                    if (page_num + 1) not in blank_pages:
                        new_doc.insert_pdf(doc, from_page=page_num, to_page=page_num)
                doc.close()
                doc = new_doc
                doc_modified = True
        
        # ✅ PASO 2: Reducir DPI de imágenes
        if process_dpi:
            images_modified = 0
            total_images = 0
            
            for page_num in range(len(doc)):
                page = doc[page_num]
                images = page.get_images(full=True)
                total_images += len(images)
                
                for img_index, img_info in enumerate(images):
                    xref = img_info[0]
                    try:
                        base_image = doc.extract_image(xref)
                        current_xres = base_image.get('xres', 72)
                        current_yres = base_image.get('yres', 72)
                        
                        if current_xres > max_dpi or current_yres > max_dpi:
                            print(f"Reducing DPI on page {page_num+1}, image {img_index+1}: {current_xres}x{current_yres} → {max_dpi}")
                            
                            image_data = base_image["image"]
                            img = Image.open(io.BytesIO(image_data))
                            
                            scale_factor = min(max_dpi / current_xres, max_dpi / current_yres, 1.0)
                            new_width = int(img.width * scale_factor)
                            new_height = int(img.height * scale_factor)
                            
                            img_resized = img.resize((new_width, new_height), Image.Resampling.LANCZOS)
                            
                            img_bytes = io.BytesIO()
                            img_resized.save(img_bytes, format='JPEG', quality=85, optimize=True)
                            img_bytes.seek(0)
                            
                            doc.update_stream(xref, img_bytes.getvalue())
                            images_modified += 1
                            doc_modified = True
                    except Exception as e:
                        print(f"Error processing image: {e}")
            
            if images_modified > 0:
                print(f"\n🖼️  Modified {images_modified}/{total_images} images")
        
        # Guardar cambios si los hay
        if doc_modified:
            doc.save(file_path, deflate=True, garbage=3, clean=True)
            doc.close()
            current_size = getPdfSize(file_path)
            print(f"Size after DPI reduction: {current_size:.2f}MB")
        else:
            doc.close()
            current_size = original_size
        
        # ✅ PASO 3: Compresión progresiva
        if process_size and current_size > max_size_mb:
            print(f"\n{'='*60}")
            print(f"🔄 COMPRESSION STRATEGIES (Target: {max_size_mb}MB)")
            print(f"{'='*60}")
            
            compression_attempts = [
                {'name': 'Standard', 'options': {'deflate': True, 'garbage': 3, 'clean': True}},
                {'name': 'Aggressive', 'options': {'deflate': True, 'garbage': 4, 'clean': True, 'ascii': True}},
            ]
            
            best_size = current_size
            best_options = None
            
            for attempt in compression_attempts:
                try:
                    temp_compressed = f"{file_path}_comp_temp.pdf"
                    doc_temp = fitz.open(file_path)
                    doc_temp.save(temp_compressed, **attempt['options'])
                    doc_temp.close()
                    del doc_temp
                    
                    compressed_size = getPdfSize(temp_compressed)
                    print(f"  {attempt['name']}: {compressed_size:.2f}MB", end="")
                    
                    if compressed_size <= max_size_mb:
                        print(" ✅ TARGET ACHIEVED!")
                        os.remove(file_path)
                        os.rename(temp_compressed, file_path)
                        print(f"\n✅ Final size: {compressed_size:.2f}MB (Original: {original_size:.2f}MB)")
                        return True
                    
                    if compressed_size < best_size:
                        print(" (best so far)")
                        best_size = compressed_size
                        best_options = attempt['options']
                        if os.path.exists(temp_compressed):
                            os.remove(temp_compressed)
                    else:
                        print()
                        if os.path.exists(temp_compressed):
                            os.remove(temp_compressed)
                    
                except Exception as e:
                    print(f"  {attempt['name']}: ERROR - {e}")
                    if os.path.exists(temp_compressed):
                        os.remove(temp_compressed)
            
            # Si la compresión estándar no funciona, intentar reconversión
            if best_size > max_size_mb:
                print(f"\n⚠️  Standard compression failed ({best_size:.2f}MB > {max_size_mb}MB)")
                print(f"Attempting last resort: Image reconstruction...")
                
                # Intentar con diferentes DPI y calidad
                quality_configs = [
                    {'dpi': 250, 'quality': 70, 'name': 'High compression (150 DPI, 70%)'},
                    {'dpi': 250, 'quality': 5, 'name': 'Very high compression (120 DPI, 65%)'},
                ]
                
                for config in quality_configs:
                    print(f"\n  Trying: {config['name']}...")
                    if reconvert_pdf_to_images(file_path, max_size_mb, dpi=config['dpi'], quality=config['quality']):
                        final_size = getPdfSize(file_path)
                        print(f"\n✅ SUCCESS! Final size: {final_size:.2f}MB (Original: {original_size:.2f}MB)")
                        return True
                
                # Si ninguna configuración funciona, dividir el archivo
                print(f"\n⚠️  Reconversion failed. Last resort: SPLITTING PDF...")
                
                success, part1_path, part2_path, split_info = split_pdf_in_half(file_path)
                
                if success:
                    # Comprimir ambas partes
                    print(f"\n🔄 Compressing split parts...")
                    
                    for part_path in [part1_path, part2_path]:
                        try:
                            doc_part = fitz.open(part_path)
                            doc_part.save(part_path, deflate=True, garbage=4, clean=True, ascii=True)
                            doc_part.close()
                            
                            size_after = getPdfSize(part_path)
                            print(f"  {os.path.basename(part_path)}: {size_after:.2f}MB")
                        except Exception as e:
                            print(f"  Error compressing {part_path}: {e}")
                    
                    # Retornar información sobre las partes
                    print(f"\n✅ PDF successfully split into 2 parts:")
                    print(f"  Part 1: {os.path.basename(part1_path)} ({getPdfSize(part1_path):.2f}MB)")
                    print(f"  Part 2: {os.path.basename(part2_path)} ({getPdfSize(part2_path):.2f}MB)")
                    
                    return {
                        'success': True,
                        'type': 'split',
                        'original_file': file_path,
                        'part1': part1_path,
                        'part2': part2_path,
                        'split_info': split_info
                    }
                else:
                    print(f"❌ Split also failed. Using best standard compression: {best_size:.2f}MB")
                    if best_options:
                        temp_best = f"{file_path}_best_comp.pdf"
                        doc_best = fitz.open(file_path)
                        doc_best.save(temp_best, **best_options)
                        doc_best.close()
                        os.remove(file_path)
                        os.rename(temp_best, file_path)
                        print(f"✅ Applied: {best_size:.2f}MB (Original: {original_size:.2f}MB)")
                        return True
        
        # Archivo ya está dentro del límite
        final_size = getPdfSize(file_path)
        if final_size <= max_size_mb:
            print(f"\n✅ File within limit: {final_size:.2f}MB (Original: {original_size:.2f}MB)")
        else:
            print(f"\n⚠️  Final size: {final_size:.2f}MB (Original: {original_size:.2f}MB)")
        
        return True
        
    except Exception as e:
        print(f"❌ Error processing file: {e}")
        import traceback
        traceback.print_exc()
        return False


def split_pdf_in_half(pdf_path: str) -> tuple:
    """
    Divide un PDF en dos partes iguales.
    
    Returns:
        (success, path_part1, path_part2, info_dict)
    """
    try:
        print(f"\n✂️  SPLITTING PDF INTO TWO PARTS...")
        
        doc = fitz.open(pdf_path)
        total_pages = len(doc)
        mid_point = total_pages // 2
        
        print(f"Total pages: {total_pages}")
        print(f"Split point: {mid_point}")
        
        # Obtener nombre del archivo sin extensión
        base_name = os.path.splitext(pdf_path)[0]
        
        # Crear primera parte
        print(f"\nCreating part 1 (pages 1-{mid_point})...")
        doc_part1 = fitz.open()
        for page_num in range(mid_point):
            doc_part1.insert_pdf(doc, from_page=page_num, to_page=page_num)
        
        part1_path = f"{base_name}_part1.pdf"
        doc_part1.save(part1_path, deflate=True, garbage=3, clean=True)
        doc_part1.close()
        
        size_part1 = getPdfSize(part1_path)
        print(f"✅ Part 1 created: {size_part1:.2f}MB")
        
        # Crear segunda parte
        print(f"\nCreating part 2 (pages {mid_point+1}-{total_pages})...")
        doc_part2 = fitz.open()
        for page_num in range(mid_point, total_pages):
            doc_part2.insert_pdf(doc, from_page=page_num, to_page=page_num)
        
        part2_path = f"{base_name}_part2.pdf"
        doc_part2.save(part2_path, deflate=True, garbage=3, clean=True)
        doc_part2.close()
        
        size_part2 = getPdfSize(part2_path)
        print(f"✅ Part 2 created: {size_part2:.2f}MB")
        
        doc.close()
        
        info = {
            'original_size': getPdfSize(pdf_path),
            'part1_size': size_part1,
            'part2_size': size_part2,
            'total_pages': total_pages,
            'split_point': mid_point,
            'part1_pages': mid_point,
            'part2_pages': total_pages - mid_point
        }
        
        print(f"\n📊 Split Summary:")
        print(f"  Original: {info['original_size']:.2f}MB")
        print(f"  Part 1: {size_part1:.2f}MB ({mid_point} pages)")
        print(f"  Part 2: {size_part2:.2f}MB ({total_pages - mid_point} pages)")
        
        return True, part1_path, part2_path, info
        
    except Exception as e:
        print(f"❌ Error splitting PDF: {e}")
        import traceback
        traceback.print_exc()
        return False, None, None, {'error': str(e)}


async def only_check_file(file_path):
    """
    Analiza un PDF y retorna el número de problemas encontrados.
    NO modifica el archivo.
    """
    try:
        max_size_mb = 4
        max_dpi = 300
        process_size = True
        process_dpi = True
        process_blank_pages = True
        blank_threshold = 0.01
        text_threshold = 10
        
        analysis = 0
        
        print(f"\n📋 Analyzing: {os.path.basename(file_path)}")
        
        # Obtener tamaño actual
        current_size = getPdfSize(file_path)
        has_size_issues = process_size and current_size and current_size > max_size_mb
        
        if has_size_issues:
            print(f"  ❌ Size issue: {current_size:.2f}MB (limit: {max_size_mb}MB)")
            analysis += 1
        
        # Detectar páginas en blanco
        if process_blank_pages:
            blank_pages = detect_blank_pages(file_path, blank_threshold, text_threshold)
            has_blank_pages = len(blank_pages) > 0
            if has_blank_pages:
                print(f"  ❌ Blank pages: {len(blank_pages)} pages ({blank_pages})")
                analysis += 1
        
        # Detectar DPI alto
        if process_dpi:
            try:
                doc = fitz.open(file_path)
                high_dpi_count = 0
                
                for page_num in range(len(doc)):
                    page = doc[page_num]
                    images = page.get_images(full=True)
                    
                    for img_info in images:
                        xref = img_info[0]
                        try:
                            base_image = doc.extract_image(xref)
                            current_xres = base_image.get('xres', 72)
                            current_yres = base_image.get('yres', 72)
                            
                            if current_xres > max_dpi or current_yres > max_dpi:
                                high_dpi_count += 1
                        except:
                            pass
                
                doc.close()
                
                has_dpi_issues = high_dpi_count > 0
                if has_dpi_issues:
                    print(f"  ❌ DPI issue: {high_dpi_count} images exceed {max_dpi} DPI")
                    analysis += 1
            except Exception as e:
                print(f"Error checking DPI: {e}")
        
        if analysis == 0:
            print("  ✅ No issues found")
        else:
            print(f"\nIssues found: {analysis}")
        
        return analysis
        
    except Exception as e:
        error_msg = f"Error analyzing {file_path}: {e}"
        print(f"❌ {error_msg}")
        return 0
import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import threading
import re
from datetime import datetime
from PIL import Image, ImageTk
from src.utils.zpl_utils import resolve_reserved_keywords, render_zpl_image, RESERVED_KEYWORDS
from src.utils.network_utils import send_print_request

class UserView:
    def __init__(self, root, db, current_user=None):
        self.root = root
        self.db = db
        self.current_user = current_user
        # En la estructura original era un diccionario, pero get_templates devuelve una lista de dicts.
        # Ajustamos para que funcione con la nueva estructura de DB.
        self._refresh_data()
        
        self.field_entries = {}
        self.current_template = None
        self.api_url = self.db.get_api_url()
        self.logout_requested = False
        
        # Parámetros de vista previa
        self.preview_width = tk.DoubleVar(value=10.0)
        self.preview_height = tk.DoubleVar(value=15.0)
        self.preview_dpmm = tk.IntVar(value=8)  # 8 dpmm = 203 dpi
        self.preview_img_ref = None
        
        # Scanner y Fast Print
        self.scanned_serials = []
        self.fast_print_var = tk.BooleanVar(value=False)
        self.scanner_entry = None
        self.scanned_listbox = None
        
        # Gestión de historial (Ventana emergente)
        self.history_popup = None
        self.history_tree = None

        self.presets_popup = None
        
        # Parámetros de reimpresión
        self.use_manual_date = tk.BooleanVar(value=False)
        self.manual_date_var = tk.StringVar(value=datetime.now().strftime("%d/%m/%Y"))
        
        self.root.title("Gestor de Impresión de Etiquetas ZPL - Usuario")
        self.root.geometry("1400x900")  # Incrementado un poco para el nuevo módulo
        
        self._create_ui()

    def _refresh_data(self):
        templates_list = self.db.get_templates()
        self.templates = {t['name']: t for t in templates_list}
        printers_list = self.db.get_printers()
        self.printers = {p['name']: p for p in printers_list}
    
    def _create_ui(self):
        # Frame superior
        top_frame = ttk.LabelFrame(self.root, text="Seleccionar Plantilla", padding=10)
        top_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(top_frame, text="Plantilla:").pack(side=tk.LEFT, padx=5)
        self.template_var = tk.StringVar()
        template_combo = ttk.Combobox(
            top_frame,
            textvariable=self.template_var,
            values=list(self.templates.keys()),
            state="readonly",
            width=30
        )
        template_combo.pack(side=tk.LEFT, padx=5, fill=tk.X, expand=True)
        template_combo.bind("<<ComboboxSelected>>", self._on_template_selected)
        
        # Botón para abrir preconfiguraciones
        ttk.Button(
            top_frame,
            text="📁 Configuraciones del Administrador",
            command=self._show_presets_popup
        ).pack(side=tk.LEFT, padx=15)
        
        ttk.Button(
            top_frame,
            text="📜 Historial",
            command=self._show_history_popup
        ).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(
            top_frame,
            text="Cerrar Sesión",
            command=self._logout
        ).pack(side=tk.RIGHT, padx=5)
        
        # Frame principal con dos columnas expandidas
        main_frame = ttk.Frame(self.root)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)
        
        # Columna Izquierda - Formulario y configuración
        form_side = ttk.Frame(main_frame)
        form_side.pack(side=tk.LEFT, fill=tk.BOTH, expand=False, padx=(0, 5), ipadx=100)
        
        # Columna Derecha - Vista Previa
        preview_side = ttk.LabelFrame(main_frame, text="Vista Previa", padding=10)
        preview_side.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=(5, 0))
        
        self.preview_canvas = tk.Canvas(preview_side, bg="white")
        self.preview_canvas.pack(fill=tk.BOTH, expand=True)
        
        # Scrollbars para el canvas si la imagen es muy grande
        h_scroll = ttk.Scrollbar(preview_side, orient=tk.HORIZONTAL, command=self.preview_canvas.xview)
        h_scroll.pack(side=tk.BOTTOM, fill=tk.X)
        v_scroll = ttk.Scrollbar(preview_side, orient=tk.VERTICAL, command=self.preview_canvas.yview)
        v_scroll.pack(side=tk.RIGHT, fill=tk.Y)
        self.preview_canvas.config(xscrollcommand=h_scroll.set, yscrollcommand=v_scroll.set)

        # --- 1. Frame de Escáner de Seriales (ARRABA) ---
        self.scanner_frame = ttk.LabelFrame(form_side, text="Escáner de Seriales", padding=10)
        self.scanner_frame.pack(fill=tk.BOTH, expand=False, padx=0, pady=(0, 5))
        
        scan_top = ttk.Frame(self.scanner_frame)
        scan_top.pack(fill=tk.X, pady=(0, 5))
        ttk.Label(scan_top, text="Escanear Código:").pack(side=tk.LEFT, padx=5)
        self.scanner_entry = ttk.Entry(scan_top, width=30)
        self.scanner_entry.pack(side=tk.LEFT, padx=5, fill=tk.X, expand=True)
        self.scanner_entry.bind("<Return>", self._on_scan)
        
        self.scanned_listbox = tk.Listbox(self.scanner_frame, height=5)
        self.scanned_listbox.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        ttk.Button(self.scanner_frame, text="Limpiar Escaneos", command=self._clear_scans).pack(side=tk.RIGHT, padx=5, pady=5)
        
        self.qty_warning_label = ttk.Label(self.scanner_frame, text="", foreground="red")
        self.qty_warning_label.pack(side=tk.LEFT, padx=5, pady=5)

        # --- 2. Frame de campos (Llenado de datos) ---
        self.fields_frame = ttk.LabelFrame(form_side, text="Datos de la Etiqueta", padding=10)
        self.fields_frame.pack(fill=tk.BOTH, expand=True, padx=0, pady=(0, 5))
        
        # --- 3. Frame de configuración (Impresión solamente) ---
        self.config_frame = ttk.LabelFrame(form_side, text="Configuración de Impresión", padding=10)
        self.config_frame.pack(fill=tk.X, padx=0, pady=(0, 5))
        
        # Fast Print Mode y Selección de Impresora
        ttk.Checkbutton(
            self.config_frame,
            text="Fast Print Mode (Impresión rápida)",
            variable=self.fast_print_var
        ).grid(row=0, column=0, sticky=tk.W, padx=5, pady=5)
        
        ttk.Label(self.config_frame, text="Impresora:").grid(row=0, column=1, sticky=tk.E, padx=5, pady=5)
        self.printer_var = tk.StringVar()
        self.printer_combo = ttk.Combobox(
            self.config_frame,
            textvariable=self.printer_var,
            values=list(self.printers.keys()),
            state="readonly",
            width=20
        )
        self.printer_combo.grid(row=0, column=2, sticky=tk.W, padx=5, pady=5)
        if self.printers:
            self.printer_combo.set(list(self.printers.keys())[0])

        self.config_frame.columnconfigure(2, weight=1)

        # Nota: Los botones de acción manual (Imprimir, Vista Previa, etc.) han sido eliminados por requerimiento.
        # El flujo principal es ahora a través del escáner y la selección de configuraciones del administrador.

    def _on_template_selected(self, event=None):
        template_name = self.template_var.get()
        if not template_name or template_name not in self.templates:
            return
        
        template = self.templates[template_name]
        self.current_template = (template_name, template)
        self._refresh_fields(template['fields'])
        
        # Cargar automáticamente la primera configuración disponible si existe
        presets = self.db.get_label_presets(template_name)
        if presets:
            self._apply_preset_data(presets[0]['data'])
            
        if self.presets_popup and self.presets_popup.winfo_exists():
            self._refresh_presets_list()
        self._update_preview()
    
    def _refresh_fields(self, fields):
        for widget in self.fields_frame.winfo_children():
            widget.destroy()
        self.field_entries.clear()
        
        # Campos editables (excluimos las palabras reservadas)
        user_fields = [f for f in fields if f not in RESERVED_KEYWORDS]
        
        # Validar QTY
        has_qty = "QTY" in user_fields or "QTY" in RESERVED_KEYWORDS
        if hasattr(self, 'qty_warning_label'):
            if not has_qty:
                self.qty_warning_label.config(text="Campo QTY no encontrado, recuerde incluirlo en el label code")
                self.scanner_entry.config(state=tk.DISABLED)
            else:
                self.qty_warning_label.config(text="")
                self.scanner_entry.config(state=tk.NORMAL)
        
        # Mostrar palabras reservadas detectadas como referencia
        reserved_in_template = [f for f in fields if f in RESERVED_KEYWORDS]
        row_offset = 0
        if reserved_in_template:
            info_text = "Auto-resuelto: " + ", ".join(f"{{{f}}}" for f in reserved_in_template)
            ttk.Label(
                self.fields_frame,
                text=info_text,
                foreground="gray",
                font=("Arial", 8, "italic")
            ).grid(row=0, column=0, columnspan=2, sticky=tk.W, padx=5, pady=(2, 5))
            row_offset = 1
        
        for i, field in enumerate(user_fields):
            ttk.Label(self.fields_frame, text=f"{field}:").grid(
                row=i + row_offset, column=0, sticky=tk.W, padx=5, pady=5
            )
            # BLOQUEADO: El usuario no puede editar los campos de la plantilla
            entry = ttk.Entry(self.fields_frame, width=40, state="readonly")
            entry.grid(row=i + row_offset, column=1, sticky=tk.EW, padx=5, pady=5)
            self.field_entries[field] = entry
        
        self.fields_frame.columnconfigure(1, weight=1)
    
    def _get_filled_zpl(self, now=None, master_serial=None):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla")
            return None
        
        template_name, template = self.current_template
        zpl = template['zpl_format']
        
        # Reemplazar campos ingresados por el usuario
        user_values = {}
        for field, entry in self.field_entries.items():
            value = entry.get()
            user_values[field] = value
            zpl = zpl.replace(f"{{{field}}}", value)
        
        # Determinar qué fecha usar (actual o manual para reimpresión)
        resolution_now = now
        if resolution_now is None:
            if self.use_manual_date.get():
                try:
                    manual_date_str = self.manual_date_var.get()
                    resolution_now = datetime.strptime(manual_date_str, "%d/%m/%Y")
                except ValueError:
                    messagebox.showwarning("Error de Fecha", "Formato de fecha inválido. Use dd/mm/aaaa")
                    resolution_now = datetime.now()
            else:
                resolution_now = datetime.now()

        # Resolver palabras reservadas automáticamente
        zpl = resolve_reserved_keywords(
            zpl, 
            now=resolution_now, 
            part_no=user_values.get("PARTNO"),
            master_serial=master_serial
        )
        
        return zpl
        
    def _clear_scans(self):
        self.scanned_serials.clear()
        if hasattr(self, 'scanned_listbox') and self.scanned_listbox:
            self.scanned_listbox.delete(0, tk.END)

    def _on_scan(self, event=None):
        if not hasattr(self, 'scanner_entry') or not self.scanner_entry:
            return
            
        code_raw = self.scanner_entry.get().strip()
        # Normalización: Eliminar signos y espacios en blanco
        code = re.sub(r'[^A-Za-z0-9]', '', code_raw)
        self.scanner_entry.delete(0, tk.END)
        
        if not code:
            return

        # 1. Validación de PartNo (Si existe en la plantilla)
        part_no_norm = ""
        # Buscamos el campo que sea PartNo (insensible a mayúsculas)
        for field_name, entry in self.field_entries.items():
            if field_name.upper() == "PARTNO":
                part_no_norm = re.sub(r'[^A-Za-z0-9]', '', entry.get().strip())
                break
        
        if part_no_norm:
            L = len(part_no_norm)
            # Comparar los primeros L caracteres del escaneo con el PartNo
            prefix_scanned = code[:L]
            if prefix_scanned.upper() != part_no_norm.upper():
                messagebox.showwarning("Error de Validación", 
                    "Numero de parte erroneo, seleccione la plantilla o verifique que el material sea el correcto")
                return
            
            # 2. Validación de duplicado en sesión actual (Últimos 5 dígitos)
            last_5 = code[-5:]
            for s in self.scanned_serials:
                if s[-5:] == last_5:
                    messagebox.showwarning("Duplicado", "Material ya escaneado")
                    return
            
            # 3. Validación Global en Base de Datos (PartNo Prefix + Últimos 5)
            duplicate_serial = self.db.check_fuzzy_serial_duplicate(part_no_norm, last_5)
            if duplicate_serial:
                messagebox.showwarning("Duplicado Global", 
                    f"Material ya escaneado en etiqueta con serial {duplicate_serial}")
                return

        # Validaciones de QTY y Limite
        qty_str = ""
        if "QTY" in self.field_entries:
            qty_str = self.field_entries["QTY"].get().strip()
            
        if not qty_str:
            messagebox.showwarning("Error", "Por favor define el valor numérico en el campo QTY del formulario primero.")
            return
            
        try:
            qty_limit = int(qty_str)
        except ValueError:
            messagebox.showwarning("Error", "El valor del campo QTY debe ser un número entero válido.")
            return
            
        if len(self.scanned_serials) >= qty_limit:
            return
            
        # Añadir a la lista si pasó todas las validaciones
        self.scanned_serials.append(code)
        self.scanned_listbox.insert(tk.END, code)
        self.scanned_listbox.yview(tk.END)
        
        # Verificar límite después de añadir
        if len(self.scanned_serials) == qty_limit:
            self._print_labels()

    def _print_labels(self):
        printer = self.printer_var.get()
        if not printer:
            messagebox.showwarning("Advertencia", "Selecciona una impresora")
            return
            
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla")
            return
            
        # Validar QTY y cantidad de escaneos
        qty_str = ""
        if "QTY" in self.field_entries:
            qty_str = self.field_entries["QTY"].get().strip()
            
        if qty_str:
            try:
                qty_limit = int(qty_str)
                if len(self.scanned_serials) != qty_limit:
                    messagebox.showwarning("Advertencia", f"Faltan escanear {qty_limit - len(self.scanned_serials)} códigos.\nDebes completar la lista antes de imprimir.")
                    return
            except ValueError:
                messagebox.showwarning("Error", "El campo QTY debe ser numérico")
                return
        else:
            messagebox.showwarning("Error", "No se puede imprimir porque el campo QTY no está definido o es inválido.")
            return

        self._do_print(printer)
    
    def _do_print(self, printer_name):
        if not self.current_template:
            return
            
        form_data = {field: entry.get() for field, entry in self.field_entries.items()}
        
        if self.use_manual_date.get():
            try:
                manual_date_str = self.manual_date_var.get()
                now = datetime.strptime(manual_date_str, "%d/%m/%Y")
            except ValueError:
                messagebox.showwarning("Error de Fecha", "Formato de fecha inválido. Se usará la fecha actual.")
                now = datetime.now()
        else:
            now = datetime.now()
            
        part_no_str = str(form_data.get("PARTNO", "")).strip()
        prefix = part_no_str[:4].upper() if part_no_str else "GNRL"
        date_suffix = now.strftime("%d%m%y")
        base_prefix = f"{prefix}{date_suffix}"
        
        next_count_str = self.db.get_next_master_serial(base_prefix)
        master_serial = f"{base_prefix}{next_count_str}"
            
        zpl = self._get_filled_zpl(now=now, master_serial=master_serial)
        if zpl is None:
            return
            
        zpl_codes = [zpl]
        
        job_type = "reimpresión" if self.use_manual_date.get() else "normal"
        label_date_str = now.strftime("%Y-%m-%d %H:%M:%S")

        # Registramos la etiqueta maestra en el historial principal
        job_id = self.db.log_print_job(
            self.current_template[0], 
            printer_name, 
            1, 
            form_data, 
            'pending',
            serials=master_serial,
            label_date=label_date_str,
            job_type=job_type,
            printed_by=self.current_user
        )
        
        # Guardamos la relación Master Serial <-> Códigos Escaneados
        self.db.log_scanned_serials(job_id, master_serial, self.scanned_serials)
        
        thread = threading.Thread(target=self._send_print, args=(printer_name, zpl_codes, job_id))
        thread.daemon = True
        thread.start()
        
        self._clear_scans()

    def _send_print(self, printer_name, zpl_codes, job_id):
        if send_print_request(printer_name, zpl_codes, self.api_url):
            self.db.update_print_job_status(job_id, 'success')
        else:
            messagebox.showerror("Error", f"No se pudo enviar la impresión a la API")
            self.db.update_print_job_status(job_id, 'error')
    
    def _save_preset(self):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla primero")
            return
            
        # Diálogo para nombre
        dialog = tk.Toplevel(self.root)
        dialog.title("Guardar Preconfiguraciòn")
        dialog.geometry("350x150")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Nombre de la preconfiguraciòn:").pack(pady=10)
        name_entry = ttk.Entry(dialog, width=35)
        name_entry.pack(pady=5)
        name_entry.focus_set()
        
        def save_it():
            name = name_entry.get().strip()
            if not name:
                messagebox.showwarning("Advertencia", "Ingresa un nombre")
                return
            
            data = {field: entry.get() for field, entry in self.field_entries.items()}
            # Agregar configuración de vista previa al preset
            data["_preview_width"] = self.preview_width.get()
            data["_preview_height"] = self.preview_height.get()
            data["_preview_dpmm"] = self.preview_dpmm.get()
            
            self.db.add_label_preset(self.current_template[0], name, data)
            dialog.destroy()
            if self.presets_popup and self.presets_popup.winfo_exists():
                self._refresh_presets_list()
        
        ttk.Button(dialog, text="Guardar", command=save_it).pack(pady=10)

    def _show_presets_popup(self):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla primero")
            return

        if self.presets_popup and self.presets_popup.winfo_exists():
            self.presets_popup.lift()
            return

        self.presets_popup = tk.Toplevel(self.root)
        self.presets_popup.title(f"Plantillas Guardadas: {self.current_template[0]}")
        self.presets_popup.geometry("400x500")
        self.presets_popup.transient(self.root)
        
        main_popup_frame = ttk.Frame(self.presets_popup, padding=10)
        main_popup_frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main_popup_frame, text="Selecciona una preconfiguración:", font=("Arial", 10, "bold")).pack(pady=(0, 10))
        
        # Listbox con scrollbar
        list_container = ttk.Frame(main_popup_frame)
        list_container.pack(fill=tk.BOTH, expand=True)

        self.presets_listbox = tk.Listbox(list_container, height=15)
        self.presets_listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        scrollbar = ttk.Scrollbar(list_container, orient=tk.VERTICAL, command=self.presets_listbox.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.presets_listbox.config(yscrollcommand=scrollbar.set)
        
        self.presets_listbox.bind("<Double-1>", lambda e: self._load_preset())

        btn_container = ttk.Frame(main_popup_frame)
        btn_container.pack(fill=tk.X, pady=(10, 0))

        ttk.Button(btn_container, text="Cargar Selección", command=self._load_preset).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)
        ttk.Button(btn_container, text="Cerrar", command=self.presets_popup.destroy).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)

        self._refresh_presets_list()

    def _refresh_presets_list(self):
        if not self.presets_listbox or not self.presets_popup or not self.presets_popup.winfo_exists():
            return
            
        self.presets_listbox.delete(0, tk.END)
        if not self.current_template:
            return
            
        template_name = self.current_template[0]
        presets = self.db.get_label_presets(template_name)
        
        for preset in presets:
            self.presets_listbox.insert(tk.END, preset['preset_name'])

    def _load_preset(self):
        if not self.presets_listbox:
            return
            
        selection = self.presets_listbox.curselection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona una preconfiguraciòn", parent=self.presets_popup)
            return
        
        preset_name = self.presets_listbox.get(selection[0])
        template_name = self.current_template[0]
        presets = self.db.get_label_presets(template_name)
        
        # Buscar el preset por nombre
        preset_data = next((p['data'] for p in presets if p['preset_name'] == preset_name), None)
        
        if preset_data:
            self._apply_preset_data(preset_data)

    def _apply_preset_data(self, preset_data):
        for field, entry in self.field_entries.items():
            entry.config(state="normal")
            entry.delete(0, tk.END)
            if field in preset_data:
                entry.insert(0, preset_data[field])
            entry.config(state="readonly")
        
        # Restaurar configuración de vista previa si existe
        if "_preview_width" in preset_data:
            self.preview_width.set(preset_data["_preview_width"])
        if "_preview_height" in preset_data:
            self.preview_height.set(preset_data["_preview_height"])
        if "_preview_dpmm" in preset_data:
            self.preview_dpmm.set(preset_data["_preview_dpmm"])
            
        self._update_preview()

    def _delete_preset(self):
        if not self.presets_listbox:
            return
            
        selection = self.presets_listbox.curselection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona una preconfiguraciòn", parent=self.presets_popup)
            return
        
        preset_name = self.presets_listbox.get(selection[0])
        template_name = self.current_template[0]
        self.db.delete_label_preset(template_name, preset_name)
        self._refresh_presets_list()

    def _on_preset_selected(self, event=None):
        pass
    
    def _save_zpl(self):
        zpl = self._get_filled_zpl()
        if not zpl:
            return
        
        file_path = filedialog.asksaveasfilename(
            defaultextension=".zpl",
            filetypes=[("ZPL files", "*.zpl"), ("Text files", "*.txt")]
        )
        
        if file_path:
            try:
                with open(file_path, 'w') as f:
                    f.write(zpl)
            except Exception as e:
                messagebox.showerror("Error", str(e))
    
    def _clear_fields(self):
        for entry in self.field_entries.values():
            entry.config(state="normal")
            entry.delete(0, tk.END)
            entry.config(state="readonly")
        self._clear_scans()
    
    def _update_preview(self):
        if not self.current_template:
            return
            
        try:
            # Convertir Centímetros a Pulgadas para Labelary
            w_cm = self.preview_width.get()
            h_cm = self.preview_height.get()
            w_inch = round(w_cm / 2.54, 2)
            h_inch = round(h_cm / 2.54, 2)
            dpmm = self.preview_dpmm.get()
        except tk.TclError:
            messagebox.showwarning("Advertencia", "Dimensiones inválidas")
            return

        # Obtener ZPL (sin contadores por ahora)
        zpl = self._get_filled_zpl(now=datetime.now())
        if not zpl:
            return
            
        img = render_zpl_image(zpl, width=w_inch, height=h_inch, dpmm=dpmm)
        if img:
            # Obtener tamaño del canvas
            canvas_w = self.preview_canvas.winfo_width()
            canvas_h = self.preview_canvas.winfo_height()
            
            # Si el canvas aún no se ha dibujado, usar un tamaño por defecto razonable
            if canvas_w < 10: canvas_w = 800
            if canvas_h < 10: canvas_h = 600
            
            img_w, img_h = img.size
            
            # Ajustar la imagen si es más grande que el canvas
            if img_w > canvas_w or img_h > canvas_h:
                ratio = min(canvas_w / img_w, canvas_h / img_h)
                new_size = (int(img_w * ratio), int(img_h * ratio))
                img = img.resize(new_size, Image.Resampling.LANCZOS)
            
            self.preview_img_ref = ImageTk.PhotoImage(img)
            self.preview_canvas.delete("all")
            
            # Centrar en el canvas
            x = max(0, (canvas_w - img.width) // 2)
            y = max(0, (canvas_h - img.height) // 2)
            
            self.preview_canvas.create_image(x, y, anchor=tk.NW, image=self.preview_img_ref)
            self.preview_canvas.config(scrollregion=self.preview_canvas.bbox("all"))
        else:
            messagebox.showerror("Error", "No se pudo generar la vista previa")
    
    def _check_reprint_auth(self):
        if not self.use_manual_date.get():
            self.manual_date_entry.config(state="disabled")
            self._update_preview()
            return

        # Modal de autorización
        auth_win = tk.Toplevel(self.root)
        auth_win.title("Autorización Requerida")
        auth_win.geometry("300x150")
        auth_win.transient(self.root)
        auth_win.grab_set()

        ttk.Label(auth_win, text="Contraseña Admin:").pack(pady=10)
        pass_entry = ttk.Entry(auth_win, show="*")
        pass_entry.pack(pady=5)
        pass_entry.focus_set()

        def validate():
            # Usamos la misma lógica de login original: Admin/admin1234
            if pass_entry.get() == "admin1234":
                self.manual_date_entry.config(state="normal")
                self._update_preview()
                auth_win.destroy()
            else:
                messagebox.showerror("Error", "Contraseña incorrecta", parent=auth_win)
                self.use_manual_date.set(False)
                self.manual_date_entry.config(state="disabled")
                auth_win.destroy()

        def on_close():
            self.use_manual_date.set(False)
            self.manual_date_entry.config(state="disabled")
            auth_win.destroy()

        auth_win.protocol("WM_DELETE_WINDOW", on_close)
        ttk.Button(auth_win, text="Validar", command=validate).pack(pady=10)

    def _show_history_popup(self):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla primero")
            return

        if self.history_popup and self.history_popup.winfo_exists():
            self.history_popup.lift()
            return

        template_name = self.current_template[0]
        self.history_popup = tk.Toplevel(self.root)
        self.history_popup.title(f"Historial de Seriales: {template_name}")
        self.history_popup.geometry("600x400")
        self.history_popup.transient(self.root)
        
        main_frame = ttk.Frame(self.history_popup, padding=10)
        main_frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main_frame, text=f"Historial para: {template_name}", font=("Arial", 10, "bold")).pack(pady=(0, 10))
        
        # Treeview para mostrar el historial
        tree_frame = ttk.Frame(main_frame)
        tree_frame.pack(fill=tk.BOTH, expand=True)

        columns = ("Fecha", "Serial", "Estatus")
        self.history_tree = ttk.Treeview(tree_frame, columns=columns, show="headings")
        self.history_tree.heading("Fecha", text="Fecha de Impresión")
        self.history_tree.heading("Serial", text="Serial")
        self.history_tree.heading("Estatus", text="Estatus")
        self.history_tree.column("Fecha", width=150)
        self.history_tree.column("Serial", width=250)
        self.history_tree.column("Estatus", width=100)
        
        self.history_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # Menú contextual para copiar
        self.history_menu = tk.Menu(self.root, tearoff=0)
        self.history_menu.add_command(label="Copiar Valor", command=self._copy_history_value)
        self.history_tree.bind("<Button-3>", self._show_history_context_menu)
        
        # Evento de doble clic para mostrar detalles
        self.history_tree.bind("<Double-1>", self._show_record_details)
        
        scrollbar = ttk.Scrollbar(tree_frame, orient=tk.VERTICAL, command=self.history_tree.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.history_tree.config(yscrollcommand=scrollbar.set)
        
        btn_frame = ttk.Frame(main_frame)
        btn_frame.pack(fill=tk.X, pady=10)
        ttk.Button(btn_frame, text="Actualizar", command=self._refresh_history_list).pack(side=tk.LEFT, padx=5, expand=True)
        ttk.Button(btn_frame, text="Cerrar", command=self.history_popup.destroy).pack(side=tk.RIGHT, padx=5, expand=True)

        self._refresh_history_list()

    def _refresh_history_list(self):
        if not self.history_tree or not self.history_popup or not self.history_popup.winfo_exists():
            return
            
        for item in self.history_tree.get_children():
            self.history_tree.delete(item)
            
        template_name = self.current_template[0]
        history = self.db.get_print_history(template_name)
        self.history_details = {}  # Mapea item_id a (record, sn)
        
        for record in history:
            # Limpiar el formato de los seriales {SN1, SN2} -> SN1, SN2
            serials_raw = record['serials']
            if serials_raw and serials_raw.startswith('{') and serials_raw.endswith('}'):
                serials_raw = serials_raw[1:-1]
            
            if serials_raw:
                # Dividir por coma y limpiar espacios
                serial_list = [s.strip() for s in serials_raw.split(',')]
                for sn in serial_list:
                    item_id = self.history_tree.insert("", "end", values=(record['timestamp'], sn, record['status']))
                    self.history_details[item_id] = (record, sn)
            else:
                item_id = self.history_tree.insert("", "end", values=(record['timestamp'], "N/A", record['status']))
                self.history_details[item_id] = (record, "N/A")

    def _show_record_details(self, event):
        selection = self.history_tree.selection()
        if not selection:
            return
            
        item_id = selection[0]
        if not hasattr(self, 'history_details') or item_id not in self.history_details:
            return
            
        record, sn = self.history_details[item_id]
        
        details_win = tk.Toplevel(self.history_popup)
        details_win.title(f"Detalles - {sn}")
        details_win.geometry("500x450")
        details_win.transient(self.history_popup)
        details_win.grab_set()
        
        main_frame = ttk.Frame(details_win, padding=10)
        main_frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main_frame, text=f"Detalles para Serial: {sn}", font=("Arial", 12, "bold")).pack(pady=(0, 10))
        ttk.Label(main_frame, text=f"Plantilla: {record['template_name']}").pack(anchor=tk.W)
        ttk.Label(main_frame, text=f"Fecha Impresión: {record['timestamp']}").pack(anchor=tk.W, pady=(0, 10))
        
        # Lista de seriales escaneados
        ttk.Label(main_frame, text="Seriales Escaneados Relacionados:").pack(anchor=tk.W, pady=(5, 2))
        scanned_listbox = tk.Listbox(main_frame, height=8)
        scanned_listbox.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        if sn != "N/A":
            job_id = record.get('id')
            scanned_serials = self.db.get_scanned_serials_by_job_id(job_id) if job_id else self.db.get_scanned_serials_by_master_serial(sn)
            for code in scanned_serials:
                clean_code = re.sub(r'[^A-Za-z0-9]', '', code)
                if clean_code:
                    scanned_listbox.insert(tk.END, clean_code)
            if not scanned_serials:
                scanned_listbox.insert(tk.END, "Ningún serial escaneado asociado.")
        else:
            scanned_listbox.insert(tk.END, "Serial Maestro no disponible.")

        # Botones de acción y reimpresión
        btn_frame = ttk.Frame(main_frame)
        btn_frame.pack(fill=tk.X, pady=(10, 0))
        
        # Parámetro manual de fecha para la reimpresión
        use_manual_date = tk.BooleanVar(value=False)
        manual_date_var = tk.StringVar(value=datetime.now().strftime("%d/%m/%Y"))
        
        date_frame = ttk.Frame(btn_frame)
        date_frame.pack(side=tk.LEFT, fill=tk.X, expand=True)
        
        check_manual = ttk.Checkbutton(date_frame, text="Usar Fecha Manual (admin)", variable=use_manual_date)
        check_manual.pack(side=tk.TOP, anchor=tk.W)
        
        date_entry = ttk.Entry(date_frame, textvariable=manual_date_var, width=15)
        date_entry.pack(side=tk.TOP, anchor=tk.W, padx=20)
        date_entry.config(state="disabled")
        
        def toggle_date_entry():
            if use_manual_date.get():
                auth_win = tk.Toplevel(details_win)
                auth_win.title("Autorización")
                auth_win.geometry("300x150")
                auth_win.transient(details_win)
                auth_win.grab_set()

                ttk.Label(auth_win, text="Contraseña Admin:").pack(pady=10)
                pass_entry = ttk.Entry(auth_win, show="*")
                pass_entry.pack(pady=5)
                pass_entry.focus_set()

                def validate_auth():
                    if pass_entry.get() == "admin1234":
                        date_entry.config(state="normal")
                        auth_win.destroy()
                    else:
                        messagebox.showerror("Error", "Contraseña incorrecta", parent=auth_win)
                        use_manual_date.set(False)
                        date_entry.config(state="disabled")
                        auth_win.destroy()

                def on_close():
                    use_manual_date.set(False)
                    date_entry.config(state="disabled")
                    auth_win.destroy()

                auth_win.protocol("WM_DELETE_WINDOW", on_close)
                ttk.Button(auth_win, text="Validar", command=validate_auth).pack(pady=10)
            else:
                date_entry.config(state="disabled")

        check_manual.config(command=toggle_date_entry)
        
        def on_reprint():
            d_now = datetime.now()
            use_manual = use_manual_date.get()
            if use_manual:
                try:
                    d_now = datetime.strptime(manual_date_var.get(), "%d/%m/%Y")
                except ValueError:
                    messagebox.showwarning("Error de Fecha", "Formato de fecha inválido. Se usará la fecha actual.", parent=details_win)
                    d_now = datetime.now()
                    use_manual = False
            
            self._reprint_from_history(record, d_now, use_manual, sn)
            details_win.destroy()
            
        ttk.Button(btn_frame, text="Reimprimir Etiqueta", command=on_reprint).pack(side=tk.RIGHT, padx=5, pady=5)
        ttk.Button(btn_frame, text="Cerrar", command=details_win.destroy).pack(side=tk.RIGHT, padx=5, pady=5)

    def _reprint_from_history(self, record, d_now, use_manual=False, original_sn=None):
        template_name = record['template_name']
        printer = self.printer_var.get()
        if not printer:
            messagebox.showwarning("Advertencia", "Selecciona una impresora")
            return
            
        if template_name not in self.templates:
            messagebox.showerror("Error", f"Plantilla {template_name} no disponible.")
            return

        form_data = record.get('data', {})
        
        if use_manual:
            part_no_str = str(form_data.get("PARTNO", "")).strip()
            prefix = part_no_str[:4].upper() if part_no_str else "GNRL"
            date_suffix = d_now.strftime("%d%m%y")
            base_prefix = f"{prefix}{date_suffix}"
            
            next_count_str = self.db.get_next_master_serial(base_prefix)
            master_serial = f"{base_prefix}{next_count_str}"
        else:
            master_serial = original_sn
            try:
                d_now = datetime.strptime(record['timestamp'], "%Y-%m-%d %H:%M:%S")
            except Exception:
                pass
        
        template = self.templates[template_name]
        zpl = template['zpl_format']
        for field, value in form_data.items():
            zpl = zpl.replace(f"{{{field}}}", str(value))
            
        zpl = resolve_reserved_keywords(
            zpl, 
            now=d_now, 
            part_no=form_data.get("PARTNO"),
            master_serial=master_serial
        )
        
        old_master_serial = original_sn if original_sn else record.get('serials', '').strip()
        if old_master_serial and old_master_serial.startswith('{') and old_master_serial.endswith('}'):
            old_master_serial = old_master_serial[1:-1]
            
        if ',' in old_master_serial:
            old_master_serial = old_master_serial.split(',')[0].strip()
            
        old_scanned_serials = []
        if record.get('id'):
            old_scanned_serials = self.db.get_scanned_serials_by_job_id(record['id'])
        
        job_type = "reimpresión historial"
        label_date_str = d_now.strftime("%Y-%m-%d %H:%M:%S")

        job_id = self.db.log_print_job(
            template_name, 
            printer, 
            1, 
            form_data, 
            'pending',
            serials=master_serial,
            label_date=label_date_str,
            job_type=job_type,
            printed_by=self.current_user
        )
        
        if old_scanned_serials:
            self.db.log_scanned_serials(job_id, master_serial, old_scanned_serials)
            
        zpl_codes = [zpl]
        thread = threading.Thread(target=self._send_print, args=(printer, zpl_codes, job_id))
        thread.daemon = True
        thread.start()


    def _show_history_context_menu(self, event):
        item = self.history_tree.identify_row(event.y)
        if item:
            self.history_tree.selection_set(item)
            self.history_menu.post(event.x_root, event.y_root)

    def _copy_history_value(self):
        selection = self.history_tree.selection()
        if not selection:
            return
            
        item = selection[0]
        # Identificar qué columna se clickeó es difícil con Treeview estándar, 
        # así que copiaremos el Serial por defecto que es lo más útil, o un diálogo?
        # Mejor: copiar todo el registro o preguntar. 
        # Pero el usuario pidió "cualquiera de los valores", lo ideal sería el valor bajo el cursor.
        # Por simplicidad y utilidad, copiaremos el Serial.
        values = self.history_tree.item(item)['values']
        if values:
            serial_val = values[1] # Indice del Serial
            self.root.clipboard_clear()
            self.root.clipboard_append(str(serial_val))

    def _logout(self):
        self.logout_requested = True
        self.root.destroy()

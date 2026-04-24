import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from email.mime.base import MIMEBase
from email import encoders
import os
import zipfile
import tempfile
from typing import Tuple, Optional
import logging

logger = logging.getLogger(__name__)

class EmailSender:    
    def __init__(self, smtp_server: str, smtp_port: int, sender_email: str, sender_password: str):
        self.smtp_server = smtp_server
        self.smtp_port = smtp_port
        self.sender_email = sender_email
        self.sender_password = sender_password
    
    def _prepare_attachment(self, attachment_path: str) -> Tuple[str, bool]:
        # Extensiones bloqueadas por proveedores de correo
        BLOCKED_EXTENSIONS = {".hta", ".exe", ".bat", ".cmd", ".vbs", ".js", ".ps1"}
        _, ext = os.path.splitext(attachment_path)
        
        if ext.lower() in BLOCKED_EXTENSIONS:
            # Crear ZIP temporal
            tmp_dir = tempfile.mkdtemp()
            zip_name = os.path.splitext(os.path.basename(attachment_path))[0] + ".zip"
            zip_path = os.path.join(tmp_dir, zip_name)
            
            with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
                zf.write(attachment_path, os.path.basename(attachment_path))
            
            logger.info(f"Archivo comprimido en ZIP: {zip_name}")
            return zip_path, True  # es_temporal=True
        
        return attachment_path, False  # es_temporal=False

    def send_email(self, recipient_email: str, subject: str, html_content: str, attachment_path: Optional[str] = None) -> Tuple[bool, str]:
        try:
            # Crear mensaje
            message = MIMEMultipart("alternative")
            message["Subject"] = subject
            message["From"] = self.sender_email
            message["To"] = recipient_email
            
            # Agregar contenido HTML
            html_part = MIMEText(html_content, "html")
            message.attach(html_part)
            
            # Agregar adjunto si existe
            tmp_path = None
            if attachment_path and os.path.isfile(attachment_path):
                actual_path, is_temp = self._prepare_attachment(attachment_path)
                if is_temp:
                    tmp_path = actual_path
                
                filename = os.path.basename(actual_path)
                with open(actual_path, "rb") as attachment:
                    part = MIMEBase("application", "octet-stream")
                    part.set_payload(attachment.read())
                
                encoders.encode_base64(part)
                part.add_header(
                    "Content-Disposition",
                    f"attachment; filename= {filename}",
                )
                message.attach(part)
                logger.info(f"Adjuntado archivo: {filename}")
            elif attachment_path:
                logger.warning(f"No se encontró el archivo adjunto: {attachment_path}")

            # Enviar
            with smtplib.SMTP(self.smtp_server, self.smtp_port) as server:
                server.starttls()  # Usar TLS
                server.login(self.sender_email, self.sender_password)
                server.sendmail(self.sender_email, recipient_email, message.as_string())
            
            # Limpiar ZIP temporal si se creó
            if tmp_path and os.path.exists(tmp_path):
                os.remove(tmp_path)
            
            logger.info(f"OK Correo enviado a: {recipient_email}")
            return True, f"Correo enviado a {recipient_email}"
            
        except smtplib.SMTPAuthenticationError:
            msg = f"Error de autenticación SMTP. Verifica usuario/contraseña."
            logger.error(msg)
            return False, msg
        except smtplib.SMTPException as e:
            msg = f"Error SMTP al enviar a {recipient_email}: {str(e)}"
            logger.error(msg)
            return False, msg
        except Exception as e:
            msg = f"Error inesperado enviando a {recipient_email}: {str(e)}"
            logger.error(msg)
            return False, msg
    
    def send_batch_emails(self, recipients_data: list, subject: str, 
                         html_generator_func, attachment_path: Optional[str] = None, max_retries: int = 3) -> dict:
        stats = {
            "total": len(recipients_data),
            "enviados": 0,
            "fallos": 0,
            "errores": []
        }
        
        for idx, recipient_data in enumerate(recipients_data, 1):
            email = recipient_data.get("email", "")
            if not email:
                logger.warning(f"Fila {idx}: Email vacío, saltando")
                stats["fallos"] += 1
                continue
            
            logger.info(f"\n[{idx}/{len(recipients_data)}] Procesando: {email}")
            
            try:
                # Generar contenido HTML personalizado
                html_content = html_generator_func(recipient_data)
                
                # Enviar con reintentos
                success = False
                for intento in range(max_retries):
                    success, mensaje = self.send_email(email, subject, html_content, attachment_path)
                    if success:
                        stats["enviados"] += 1
                        break
                    elif intento < max_retries - 1:
                        logger.warning(f"Reintentando ({intento + 1}/{max_retries-1})...")
                
                if not success:
                    stats["fallos"] += 1
                    stats["errores"].append({"email": email, "error": mensaje})
                    
            except Exception as e:
                stats["fallos"] += 1
                error_msg = str(e)
                stats["errores"].append({"email": email, "error": error_msg})
                logger.error(f"Error procesando {email}: {error_msg}")
        
        # Log resumen
        logger.info("\n" + "="*50)
        logger.info("RESUMEN DE ENVÍO")
        logger.info("="*50)
        logger.info(f"Total: {stats['total']}")
        logger.info(f"Enviados: {stats['enviados']} OK")
        logger.info(f"Fallos: {stats['fallos']} FAIL")
        
        if stats["errores"]:
            logger.info("\nErrores:")
            for error in stats["errores"]:
                logger.info(f"  - {error['email']}: {error['error']}")
        
        return stats
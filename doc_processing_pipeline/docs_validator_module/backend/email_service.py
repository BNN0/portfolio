from typing import List
from datetime import datetime
import os
import ssl
import smtplib


class FieldComparison:
    def __init__(self, campo: str, logistica: str, sat: str, estado: str):
        self.campo = campo
        self.logistica = logistica
        self.sat = sat
        self.estado = estado


class EmailService:
    def __init__(self):
        self.from_email = os.getenv("FROM_EMAIL", "noreply@tuempresa.com")
    
    def generate_email_html(
        self, 
        invoice_number: str, 
        fields: List[FieldComparison],
        validation_id: str
    ) -> str:
        # Contar errores y aciertos
        errors = [f for f in fields if f.estado == "danger"]
        successes = [f for f in fields if f.estado == "success"]
        
        # Generar filas de la tabla
        rows_html = ""
        for field in fields:
            bg_color = "#f0fdf4" if field.estado == "success" else "#fef2f2"
            icon = "✓" if field.estado == "success" else "✗"
            icon_color = "#10b981" if field.estado == "success" else "#ef4444"
            
            rows_html += f"""
            <tr style="background-color: {bg_color};">
                <td style="padding: 12px; border-bottom: 1px solid #e5e7eb; font-weight: 600;">
                    {field.campo}
                </td>
                <td style="padding: 12px; border-bottom: 1px solid #e5e7eb;">
                    {field.logistica}
                </td>
                <td style="padding: 12px; border-bottom: 1px solid #e5e7eb;">
                    {field.sat}
                </td>
                <td style="padding: 12px; border-bottom: 1px solid #e5e7eb; text-align: center; color: {icon_color}; font-size: 20px; font-weight: bold;">
                    {icon}
                </td>
            </tr>
            """
        
        html = f"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Observaciones de Validación - Factura {invoice_number}</title>
        </head>
        <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px;">
            
            <!-- Header -->
            <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;">
                <h1 style="margin: 0; font-size: 28px;">Observaciones de Validación</h1>
                <p style="margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;">Factura {invoice_number}</p>
            </div>
            
            <!-- Body -->
            <div style="background: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none;">
                
                <!-- Resumen -->
                <div style="background: #f9fafb; padding: 20px; border-radius: 8px; margin-bottom: 30px;">
                    <h2 style="margin: 0 0 15px 0; color: #1f2937; font-size: 20px;">📊 Resumen de Validación</h2>
                    <p style="margin: 5px 0; font-size: 15px;">
                        <strong>ID de Validación:</strong> {validation_id}
                    </p>
                    <p style="margin: 5px 0; font-size: 15px;">
                        <strong>Fecha:</strong> {datetime.now().strftime("%d/%m/%Y %H:%M")}
                    </p>
                    <div style="display: flex; gap: 20px; margin-top: 15px;">
                        <div style="flex: 1; background: #d1fae5; padding: 15px; border-radius: 6px; text-align: center;">
                            <div style="font-size: 24px; font-weight: bold; color: #065f46;">{len(successes)}</div>
                            <div style="font-size: 14px; color: #047857;">Campos Correctos</div>
                        </div>
                        <div style="flex: 1; background: #fee2e2; padding: 15px; border-radius: 6px; text-align: center;">
                            <div style="font-size: 24px; font-weight: bold; color: #991b1b;">{len(errors)}</div>
                            <div style="font-size: 14px; color: #dc2626;">Discrepancias</div>
                        </div>
                    </div>
                </div>
                
                <!-- Tabla de Comparación -->
                <h2 style="color: #1f2937; font-size: 20px; margin-bottom: 15px;">📋 Detalle de Comparación</h2>
                
                <table style="width: 100%; border-collapse: collapse; margin-bottom: 30px; border: 1px solid #e5e7eb;">
                    <thead>
                        <tr style="background-color: #f3f4f6;">
                            <th style="padding: 12px; text-align: left; font-size: 12px; text-transform: uppercase; color: #6b7280; border-bottom: 2px solid #e5e7eb;">
                                Campo
                            </th>
                            <th style="padding: 12px; text-align: left; font-size: 12px; text-transform: uppercase; color: #6b7280; border-bottom: 2px solid #e5e7eb;">
                                Factura Logística
                            </th>
                            <th style="padding: 12px; text-align: left; font-size: 12px; text-transform: uppercase; color: #6b7280; border-bottom: 2px solid #e5e7eb;">
                                Documento SAT
                            </th>
                            <th style="padding: 12px; text-align: center; font-size: 12px; text-transform: uppercase; color: #6b7280; border-bottom: 2px solid #e5e7eb;">
                                Estado
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows_html}
                    </tbody>
                </table>
                
                <!-- Recomendaciones -->
                {self._generate_recommendations_html(errors) if errors else ""}
                
                <!-- Footer de Acción -->
                <div style="background: #eff6ff; padding: 20px; border-radius: 8px; border-left: 4px solid #3b82f6;">
                    <p style="margin: 0 0 10px 0; font-size: 14px; color: #1e40af;">
                        <strong>⚠️ Acción Requerida:</strong>
                    </p>
                    <p style="margin: 0; font-size: 14px; color: #1e3a8a;">
                        Por favor, revise las discrepancias encontradas y tome las acciones necesarias para corregir los datos antes de procesar esta factura.
                    </p>
                </div>
            </div>
            
            <!-- Footer -->
            <div style="background: #f9fafb; padding: 20px; text-align: center; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;">
                <p style="margin: 0; font-size: 12px; color: #6b7280;">
                    Este es un correo automático generado por el Sistema de Validación de Facturas.
                </p>
                <p style="margin: 5px 0 0 0; font-size: 12px; color: #6b7280;">
                    © {datetime.now().year} Tu Empresa. Todos los derechos reservados.
                </p>
            </div>
            
        </body>
        </html>
        """
        
        return html
    
    def _generate_recommendations_html(self, errors: List[FieldComparison]) -> str:
        if not errors:
            return ""
        
        recommendations = []
        for error in errors:
            if "Total" in error.campo or "Subtotal" in error.campo:
                recommendations.append("Verifique los cálculos aritméticos y los importes registrados.")
            elif "RFC" in error.campo:
                recommendations.append("Confirme que el RFC esté correctamente capturado en ambos sistemas.")
            elif "Fecha" in error.campo:
                recommendations.append("Valide las fechas de emisión en ambos documentos.")
            elif "Proveedor" in error.campo or "Razón" in error.campo:
                recommendations.append("Verifique que la razón social coincida exactamente en ambos documentos.")
        
        # Eliminar duplicados
        recommendations = list(set(recommendations))
        
        recs_html = ""
        for rec in recommendations:
            recs_html += f'<li style="margin: 8px 0; color: #374151;">{rec}</li>'
        
        return f"""
        <div style="background: #fef3c7; padding: 20px; border-radius: 8px; margin-bottom: 20px; border-left: 4px solid #f59e0b;">
            <h3 style="margin: 0 0 10px 0; color: #92400e; font-size: 16px;">💡 Recomendaciones</h3>
            <ul style="margin: 0; padding-left: 20px;">
                {recs_html}
            </ul>
        </div>
        """
    
    async def send_email_sendgrid(
        self,
        to_email: str,
        invoice_number: str,
        fields: List[FieldComparison],
        validation_id: str
    ) -> bool:
        try:
            from sendgrid import SendGridAPIClient
            from sendgrid.helpers.mail import Mail
            
            html_content = self.generate_email_html(invoice_number, fields, validation_id)
            
            message = Mail(
                from_email=self.from_email,
                to_emails=to_email,
                subject=f'Observaciones de Validación - Factura {invoice_number}',
                html_content=html_content
            )
            
            sg = SendGridAPIClient(os.environ.get('SENDGRID_API_KEY'))
            response = sg.send(message)
            
            return response.status_code == 202
        except Exception as e:
            print(f"Error enviando email con SendGrid: {str(e)}")
            return False
    
    async def send_email_smtp(
        self,
        to_email: str,
        invoice_number: str,
        fields: List[FieldComparison],
        validation_id: str
    ) -> bool:
        try:
            import aiosmtplib
            from email.mime.text import MIMEText
            from email.mime.multipart import MIMEMultipart

            context = ssl.create_default_context()
            context.check_hostname = False
            context.verify_mode = ssl.CERT_NONE
            
            html_content = self.generate_email_html(invoice_number, fields, validation_id)
            
            message = MIMEMultipart("alternative")
            message["Subject"] = f'Observaciones de Validación - Factura {invoice_number}'
            message["From"] = self.from_email
            message["To"] = to_email
            
            html_part = MIMEText(html_content, "html")
            message.attach(html_part)
            
            response = await aiosmtplib.send(
                message,
                hostname=os.getenv("SMTP_HOST", "smtp.gmail.com"),
                port=int(os.getenv("SMTP_PORT", "587")),
                username="j_lopez@mitsumi.mx", #os.getenv(""),
                password="gfop drwk sqkp qnwr",#os.getenv(),
                start_tls=True,
                use_tls=False,
                tls_context=context
            )

            print(response)
            
            return True
        except Exception as e:
            print(f"Error enviando email con SMTP: {str(e)}")
            return False
    
    async def send_email_aws_ses(
        self,
        to_email: str,
        invoice_number: str,
        fields: List[FieldComparison],
        validation_id: str
    ) -> bool:
        try:
            import boto3
            from botocore.exceptions import ClientError
            
            html_content = self.generate_email_html(invoice_number, fields, validation_id)
            
            client = boto3.client(
                'ses',
                region_name=os.getenv("AWS_REGION", "us-east-1")
            )
            
            response = client.send_email(
                Source=self.from_email,
                Destination={'ToAddresses': [to_email]},
                Message={
                    'Subject': {
                        'Data': f'Observaciones de Validación - Factura {invoice_number}',
                        'Charset': 'UTF-8'
                    },
                    'Body': {
                        'Html': {
                            'Data': html_content,
                            'Charset': 'UTF-8'
                        }
                    }
                }
            )
            
            return True
        except ClientError as e:
            print(f"Error enviando email con AWS SES: {e.response['Error']['Message']}")
            return False
        except Exception as e:
            print(f"Error enviando email con AWS SES: {str(e)}")
            return False


# Instancia global del servicio
email_service = EmailService()
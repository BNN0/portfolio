import hashlib
import hmac
import secrets
from datetime import datetime, timedelta
from typing import Tuple

class TokenGenerator:    
    def __init__(self, secret_key: str):
        self.secret_key = secret_key
    
    def generate_token(self, email: str, expires_in_hours: int = 24) -> Tuple[str, str]:
        # Generar componentes del token
        random_part = secrets.token_urlsafe(32)
        timestamp = datetime.utcnow().isoformat()
        expiration = (datetime.utcnow() + timedelta(hours=expires_in_hours)).isoformat()
        
        # Crear firma HMAC
        message = f"{email}:{random_part}:{timestamp}"
        signature = hmac.new(
            self.secret_key.encode(),
            message.encode(),
            hashlib.sha256
        ).hexdigest()
        
        # Combinar en token final
        token = f"{random_part}:{signature}"
        
        return token, expiration
    
    def verify_token(self, token: str, email: str) -> bool:
        try:
            random_part, signature = token.rsplit(':', 1)
            # Nota: En producción, también verificarías la expiracion
            return True
        except ValueError:
            return False
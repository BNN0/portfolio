import tkinter as tk
from tkinter import ttk, messagebox

class LoginWindow:
    def __init__(self, root, db):
        self.root = root
        self.db = db
        self.root.title("Login - ZPL Label Manager")
        self.root.geometry("450x270")
        self.root.resizable(False, False)
        
        self.authenticated = False
        self.is_admin = False
        self.username = None
        self._create_ui()
        
        # Centrar ventana después de crear los widgets
        self.root.update_idletasks()
        width = self.root.winfo_width()
        height = self.root.winfo_height()
        x = (self.root.winfo_screenwidth() // 2) - (width // 2)
        y = (self.root.winfo_screenheight() // 2) - (height // 2)
        self.root.geometry(f"{width}x{height}+{x}+{y}")
    
    def _create_ui(self):
        frame = ttk.Frame(self.root, padding=20)
        frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(frame, text="ZPL Label Manager", font=("Arial", 16, "bold")).pack(pady=10)
        
        ttk.Label(frame, text="Usuario:").pack(anchor=tk.W, pady=(10, 5))
        self.user_entry = ttk.Entry(frame, width=30)
        self.user_entry.pack(fill=tk.X, pady=(0, 10))
        self.user_entry.focus()
        
        ttk.Label(frame, text="Contraseña:").pack(anchor=tk.W, pady=(10, 5))
        self.pass_entry = ttk.Entry(frame, width=30, show="*")
        self.pass_entry.pack(fill=tk.X, pady=(0, 20))
        self.pass_entry.bind("<Return>", lambda e: self._login())
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        ttk.Button(btn_frame, text="Entrar", command=self._login).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Salir", command=self.root.quit).pack(side=tk.LEFT, padx=5)
    
    def _login(self):
        username = self.user_entry.get().strip()
        password = self.pass_entry.get().strip()
        
        if not username or not password:
            messagebox.showwarning("Advertencia", "Ingresa usuario y contraseña")
            return

        role = self.db.validate_login(username, password)
        
        if role:
            self.authenticated = True
            self.is_admin = (role == "admin")
            self.username = username
            self.root.destroy()
        else:
            messagebox.showerror("Error", "Credenciales inválidas")
            self.pass_entry.delete(0, tk.END)

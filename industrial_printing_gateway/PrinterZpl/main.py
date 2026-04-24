import tkinter as tk
from src.database.db_handler import Database
from src.ui.login import LoginWindow
from src.ui.user_view import UserView
from src.ui.admin_view import AdminView

def main():
    # Inicializar la base de datos (se encargará de crear tablas si no existen)
    db = Database()
    
    while True:
        # Ventana de login
        root = tk.Tk()
        login = LoginWindow(root, db)
        root.mainloop()
        
        if not login.authenticated:
            break  
        
        # Ventana principal
        root = tk.Tk()
        if login.is_admin:
            view = AdminView(root, db, current_user=login.username)
        else:
            view = UserView(root, db, current_user=login.username)
        
        root.mainloop()
        
        if not hasattr(view, 'logout_requested') or not view.logout_requested:
            break

if __name__ == "__main__":
    main()
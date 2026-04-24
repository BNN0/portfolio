import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import pandas as pd
from PIL import Image, ImageTk
from src.utils.zpl_utils import render_zpl_image

class AdminView:
    def __init__(self, root, db, current_user=None):
        self.root = root
        self.db = db
        self.current_user = current_user
        self._editing_template = None
        self._editing_printer = None
        self.logout_requested = False
        
        # Parámetros de vista previa
        self.preview_width = tk.DoubleVar(value=10.0)
        self.preview_height = tk.DoubleVar(value=15.0)
        self.preview_dpmm = tk.IntVar(value=8)
        self.preview_img_ref = None
        
        # Gestión de plantillas (Ventana emergente)
        self.templates_popup = None
        self.templates_tree = None
        
        # Gestión de configuraciones de etiquetas (Presets)
        self.field_entries = {}
        self.current_template = None
        self.presets_popup = None
        self.presets_listbox = None
        
        # Seguridad de pestañas
        self.last_tab_index = 0
        self._protecting_tabs = False
        
        self.root.title("Administrador - ZPL Label Manager")
        self.root.geometry("1400x800")
        
        self._create_ui()
    
    def _create_ui(self):
        # Barra superior con botón de cerrar sesión
        header_frame = ttk.Frame(self.root)
        header_frame.pack(fill=tk.X, padx=10, pady=(10, 0))
        ttk.Label(header_frame, text="Administrador - ZPL Label Manager", font=("Arial", 10, "bold")).pack(side=tk.LEFT)
        ttk.Button(header_frame, text="Cerrar Sesión", command=self._logout).pack(side=tk.RIGHT)
        
        # Notebook con pestañas
        self.notebook = ttk.Notebook(self.root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)
        
        # Pestaña Historial (Ahora la primera)
        history_frame = ttk.Frame(self.notebook)
        self.notebook.add(history_frame, text="Historial")
        self._create_history_tab(history_frame)
        
        # Pestaña Plantillas
        templates_frame = ttk.Frame(self.notebook)
        self.notebook.add(templates_frame, text="Plantillas")
        self._create_templates_tab(templates_frame)
        
        # Pestaña Impresoras
        printers_frame = ttk.Frame(self.notebook)
        self.notebook.add(printers_frame, text="Impresoras")
        self._create_printers_tab(printers_frame)

        # Pestaña Usuarios
        users_frame = ttk.Frame(self.notebook)
        self.notebook.add(users_frame, text="Usuarios")
        self._create_users_tab(users_frame)

        # Vincular evento de cambio de pestaña para seguridad
        self.notebook.bind("<<NotebookTabChanged>>", self._on_tab_change)
    
    def _on_tab_change(self, event):
        if self._protecting_tabs:
            return
            
        current_tab = self.notebook.index(self.notebook.select())
        tab_text = self.notebook.tab(current_tab, "text")
        
        # Si la pestaña es protegida y no venimos de ella misma (ya validado)
        if tab_text in ["Plantillas", "Impresoras", "Usuarios"]:
            # Pedir código de seguridad
            access_granted = self._show_password_dialog()
            
            if access_granted:
                self.last_tab_index = current_tab
            else:
                # Si cancela o falla, regresa a la pestaña anterior
                self._protecting_tabs = True
                self.notebook.select(self.last_tab_index)
                self.root.after(10, lambda: setattr(self, '_protecting_tabs', False))
        else:
            # Historial es libre
            self.last_tab_index = current_tab

    def _show_password_dialog(self):
        result = [False] # Uso de lista para mutar desde el cierre de la ventana
        
        dialog = tk.Toplevel(self.root)
        dialog.title("Acceso Protegido")
        dialog.geometry("300x150")
        dialog.resizable(False, False)
        dialog.transient(self.root)
        dialog.grab_set()
        
        # Centrar ventana respecto al root
        root_x = self.root.winfo_x()
        root_y = self.root.winfo_y()
        root_w = self.root.winfo_width()
        root_h = self.root.winfo_height()
        dialog.geometry(f"+{root_x + root_w//2 - 150}+{root_y + root_h//2 - 75}")

        ttk.Label(dialog, text="Ingrese el código de acceso:", font=("Arial", 10)).pack(pady=10)
        
        password_var = tk.StringVar()
        entry = ttk.Entry(dialog, textvariable=password_var, show="*", width=25)
        entry.pack(pady=5)
        entry.focus_set()

        def validate(event=None):
            if password_var.get() == "MAM&26+":
                result[0] = True
                dialog.destroy()
            else:
                messagebox.showerror("Error", "Código de acceso incorrecto.")
                password_var.set("") # Limpiar para reintentar
                entry.focus_set()

        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(pady=10)
        
        ttk.Button(btn_frame, text="Acceder", command=validate).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Cancelar", command=dialog.destroy).pack(side=tk.LEFT, padx=5)
        
        entry.bind("<Return>", validate)
        
        self.root.wait_window(dialog)
        return result[0]

    def _verify_user_password(self):
        result = [False]
        
        dialog = tk.Toplevel(self.root)
        dialog.title("Confirmar Identidad")
        dialog.geometry("350x150")
        dialog.resizable(False, False)
        dialog.transient(self.root)
        dialog.grab_set()
        
        # Centrar ventana
        root_x = self.root.winfo_x()
        root_y = self.root.winfo_y()
        root_w = self.root.winfo_width()
        root_h = self.root.winfo_height()
        dialog.geometry(f"+{root_x + root_w//2 - 175}+{root_y + root_h//2 - 75}")

        ttk.Label(dialog, text=f"Administrador: {self.current_user}", font=("Arial", 10, "bold")).pack(pady=(10, 5))
        ttk.Label(dialog, text="Ingrese su contraseña para autorizar la eliminación:", font=("Arial", 9)).pack(pady=5)
        
        password_var = tk.StringVar()
        entry = ttk.Entry(dialog, textvariable=password_var, show="*", width=30)
        entry.pack(pady=5)
        entry.focus_set()

        def validate(event=None):
            # Validar contra la base de datos usando el usuario actual
            if self.db.validate_login(self.current_user, password_var.get()):
                result[0] = True
                dialog.destroy()
            else:
                messagebox.showerror("Error", "Contraseña incorrecta.")
                password_var.set("")
                entry.focus_set()

        btn_frame = ttk.Frame(dialog)
        btn_frame.pack(pady=10)
        
        ttk.Button(btn_frame, text="Autorizar", command=validate).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Cancelar", command=dialog.destroy).pack(side=tk.LEFT, padx=5)
        
        entry.bind("<Return>", validate)
        
        self.root.wait_window(dialog)
        return result[0]
    
    def _create_templates_tab(self, parent):
        # Dividir la pestaña en dos columnas: Izquierda (Formulario) y Derecha (Vista Previa)
        paned = ttk.PanedWindow(parent, orient=tk.HORIZONTAL)
        paned.pack(fill=tk.BOTH, expand=True)
        
        left_side = ttk.Frame(paned, padding=10)
        right_side = ttk.Frame(paned, padding=10)
        paned.add(left_side, weight=1)
        paned.add(right_side, weight=1)

        # Formulario de Plantilla (Nombre, ZPL, Variables)
        self.form_frame = ttk.LabelFrame(left_side, text="Definición de Plantilla", padding=10)
        self.form_frame.pack(fill=tk.X, expand=False)
        
        # Botón para mostrar el listado de plantillas
        ttk.Button(
            self.form_frame,
            text="📋 Ver Listado de Plantillas",
            command=self._show_templates_popup
        ).grid(row=0, column=0, columnspan=2, sticky=tk.W, padx=5, pady=(0, 10))

        ttk.Label(self.form_frame, text="Nombre:").grid(row=1, column=0, sticky=tk.W, padx=5, pady=2)
        self.template_name = ttk.Entry(self.form_frame, width=30)
        self.template_name.grid(row=1, column=1, sticky=tk.EW, padx=5, pady=2)
        
        ttk.Label(self.form_frame, text="Variables:").grid(row=2, column=0, sticky=tk.W, padx=5, pady=2)
        self.template_fields = ttk.Entry(self.form_frame, width=30)
        self.template_fields.grid(row=2, column=1, sticky=tk.EW, padx=5, pady=2)
        
        ttk.Label(self.form_frame, text="ZPL:").grid(row=3, column=0, sticky=tk.NW, padx=5, pady=2)
        self.template_zpl = tk.Text(self.form_frame, width=40, height=8)
        self.template_zpl.grid(row=3, column=1, sticky=tk.NSEW, padx=5, pady=2)
        
        # Botones del formulario
        form_btn_container = ttk.Frame(self.form_frame)
        form_btn_container.grid(row=6, column=1, sticky=tk.E, padx=5, pady=5)
        
        self.btn_add_template = ttk.Button(form_btn_container, text="Agregar", command=self._add_template)
        self.btn_add_template.pack(side=tk.LEFT, padx=2)
        
        self.btn_save_edit = ttk.Button(form_btn_container, text="Guardar", command=self._save_template_edit)
        self.btn_cancel_edit = ttk.Button(form_btn_container, text="Cancelar", command=self._cancel_edit_template)
        
        self.form_frame.columnconfigure(1, weight=1)
        

        # --- NUEVO: Módulo de Llenado de Datos (Configuración) ---
        self.config_data_frame = ttk.LabelFrame(left_side, text="Predatos / Configuración de la Etiqueta", padding=10)
        self.config_data_frame.pack(fill=tk.BOTH, expand=True, pady=(10, 0))
        
        # Contenedor para campos dinámicos
        self.fields_frame = ttk.Frame(self.config_data_frame)
        self.fields_frame.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(self.fields_frame, text="Selecciona una plantilla para configurar sus datos.", foreground="gray").pack(pady=10)

        # Botones de gestión de configuraciones
        config_btn_frame = ttk.Frame(self.config_data_frame)
        config_btn_frame.pack(fill=tk.X, pady=(10, 0))
        
        ttk.Button(config_btn_frame, text="📁 Ver Configuraciones Guardadas", command=self._show_presets_popup).pack(side=tk.LEFT, padx=5)
        ttk.Button(config_btn_frame, text="💾 Guardar como Configuración", command=self._save_preset).pack(side=tk.LEFT, padx=5)
        ttk.Button(config_btn_frame, text="🔄 Probar Preview", command=self._update_preview).pack(side=tk.RIGHT, padx=5)
        
        # Área de Vista Previa (Lado Derecho)
        preview_container = ttk.LabelFrame(right_side, text="Vista Previa del Diseño", padding=10)
        preview_container.pack(fill=tk.BOTH, expand=True)
        
        self.preview_canvas = tk.Canvas(preview_container, bg="white")
        self.preview_canvas.pack(fill=tk.BOTH, expand=True)
        
        # Scrollbars para el canvas
        h_scroll = ttk.Scrollbar(preview_container, orient=tk.HORIZONTAL, command=self.preview_canvas.xview)
        h_scroll.pack(side=tk.BOTTOM, fill=tk.X)
        v_scroll = ttk.Scrollbar(preview_container, orient=tk.VERTICAL, command=self.preview_canvas.yview)
        v_scroll.pack(side=tk.RIGHT, fill=tk.Y)
        self.preview_canvas.config(xscrollcommand=h_scroll.set, yscrollcommand=v_scroll.set)
        
        # No intentamos cargar listas todavía porque el popup no existe hasta abrirse
    
    def _create_printers_tab(self, parent):
        api_frame = ttk.LabelFrame(parent, text="Configuración de API", padding=10)
        api_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(api_frame, text="URL del API:").pack(side=tk.LEFT, padx=5)
        self.api_url_var = tk.StringVar(value=self.db.get_api_url())
        api_entry = ttk.Entry(api_frame, textvariable=self.api_url_var, width=50)
        api_entry.pack(side=tk.LEFT, padx=5, fill=tk.X, expand=True)
        ttk.Button(api_frame, text="Guardar", command=self._save_api_url).pack(side=tk.LEFT, padx=5)
        
        # Formulario (modo agregar / editar)
        self.printer_form_frame = ttk.LabelFrame(parent, text="Nueva Impresora", padding=10)
        self.printer_form_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(self.printer_form_frame, text="Nombre:").grid(row=0, column=0, sticky=tk.W, padx=5, pady=5)
        self.printer_name = ttk.Entry(self.printer_form_frame, width=30)
        self.printer_name.grid(row=0, column=1, sticky=tk.EW, padx=5, pady=5)
        
        ttk.Label(self.printer_form_frame, text="IP Address:").grid(row=0, column=2, sticky=tk.W, padx=5, pady=5)
        self.printer_ip = ttk.Entry(self.printer_form_frame, width=20)
        self.printer_ip.grid(row=0, column=3, sticky=tk.EW, padx=5, pady=5)
        
        ttk.Label(self.printer_form_frame, text="Puerto:").grid(row=1, column=0, sticky=tk.W, padx=5, pady=5)
        self.printer_port = ttk.Spinbox(self.printer_form_frame, from_=1, to=65535, width=10)
        self.printer_port.set(9100)
        self.printer_port.grid(row=1, column=1, sticky=tk.W, padx=5, pady=5)
        
        ttk.Label(self.printer_form_frame, text="Descripción:").grid(row=1, column=2, sticky=tk.W, padx=5, pady=5)
        self.printer_desc = ttk.Entry(self.printer_form_frame, width=20)
        self.printer_desc.grid(row=1, column=3, sticky=tk.EW, padx=5, pady=5)
        
        # Botones del formulario (cambian según el modo)
        printer_btn_frame = ttk.Frame(self.printer_form_frame)
        printer_btn_frame.grid(row=2, column=3, sticky=tk.E, padx=5, pady=10)
        
        self.btn_add_printer = ttk.Button(
            printer_btn_frame,
            text="Agregar Impresora",
            command=self._add_printer
        )
        self.btn_add_printer.pack(side=tk.LEFT, padx=2)
        
        self.btn_save_printer_edit = ttk.Button(
            printer_btn_frame,
            text="Guardar Cambios",
            command=self._save_printer_edit
        )
        # oculto hasta que haya una edición activa
        
        self.btn_cancel_printer_edit = ttk.Button(
            printer_btn_frame,
            text="Cancelar",
            command=self._cancel_edit_printer
        )
        # oculto hasta que haya una edición activa
        
        self.printer_form_frame.columnconfigure(1, weight=1)
        self.printer_form_frame.columnconfigure(3, weight=1)
        
        # Lista de impresoras
        list_frame = ttk.LabelFrame(parent, text="Impresoras Configuradas", padding=10)
        list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        columns = ("Nombre", "IP", "Puerto", "Descripción")
        self.printers_tree_main = ttk.Treeview(list_frame, columns=columns, height=10)
        self.printers_tree_main.column("#0", width=0, stretch=tk.NO)
        self.printers_tree_main.column("Nombre", anchor=tk.W, width=150)
        self.printers_tree_main.column("IP", anchor=tk.W, width=150)
        self.printers_tree_main.column("Puerto", anchor=tk.W, width=100)
        self.printers_tree_main.column("Descripción", anchor=tk.W, width=300)
        
        self.printers_tree_main.heading("#0", text="", anchor=tk.W)
        self.printers_tree_main.heading("Nombre", text="Nombre", anchor=tk.W)
        self.printers_tree_main.heading("IP", text="IP", anchor=tk.W)
        self.printers_tree_main.heading("Puerto", text="Puerto", anchor=tk.W)
        self.printers_tree_main.heading("Descripción", text="Descripción", anchor=tk.W)
        
        # Doble clic carga la impresora en el formulario para editar
        self.printers_tree_main.bind("<Double-1>", lambda e: self._load_printer_for_edit())
        self.printers_tree_main.pack(fill=tk.BOTH, expand=True)
        
        list_btn_frame = ttk.Frame(list_frame)
        list_btn_frame.pack(pady=10)
        ttk.Button(
            list_btn_frame,
            text="Editar Impresora Seleccionada",
            command=self._load_printer_for_edit
        ).pack(side=tk.LEFT, padx=5)
        ttk.Button(
            list_btn_frame,
            text="Eliminar Impresora Seleccionada",
            command=self._delete_printer
        ).pack(side=tk.LEFT, padx=5)
        
        self._refresh_printers_list()
    
    def _save_api_url(self):
        api_url = self.api_url_var.get().strip()
        if not api_url:
            messagebox.showwarning("Advertencia", "Ingresa una URL válida")
            return
        
        self.db.save_api_url(api_url)
        messagebox.showinfo("Éxito", "URL de API guardada")
    
    def _add_template(self):
        name = self.template_name.get()
        fields_str = self.template_fields.get()
        zpl = self.template_zpl.get("1.0", tk.END).strip()
        
        if not name or not fields_str or not zpl:
            messagebox.showwarning("Advertencia", "Completa todos los campos")
            return
        
        fields = [f.strip() for f in fields_str.split(",")]
        
        try:
            self.db.add_template(name, zpl, fields)
            messagebox.showinfo("Éxito", "Plantilla agregada")
            self.template_name.delete(0, tk.END)
            self.template_fields.delete(0, tk.END)
            self.template_zpl.delete("1.0", tk.END)
            if self.templates_popup and self.templates_popup.winfo_exists():
                self._refresh_templates_list()
        except Exception as e:
            messagebox.showerror("Error", f"Error al agregar plantilla: {e}")
    
    def _load_template_for_edit(self, name=None):
        if not name:
            if not self.templates_tree:
                return
            selection = self.templates_tree.selection()
            if not selection:
                messagebox.showwarning("Advertencia", "Selecciona una plantilla para editar", parent=self.templates_popup)
                return
            item = selection[0]
            name = self.templates_tree.item(item)['values'][0]
        
        templates_list = self.db.get_templates()
        template = next((t for t in templates_list if t['name'] == name), None)
        
        if not template:
            return
        
        # Rellenar el formulario
        self.template_name.config(state="normal")
        self.template_name.delete(0, tk.END)
        self.template_name.insert(0, name)
        self.template_name.config(state="readonly")   # El nombre es la PK, no se puede cambiar
        
        self.template_fields.delete(0, tk.END)
        self.template_fields.insert(0, ", ".join(template['fields']))
        
        self.template_zpl.delete("1.0", tk.END)
        self.template_zpl.insert("1.0", template['zpl_format'])
        
        # Cambiar modo del formulario a "Editar"
        self._editing_template = name
        self.form_frame.config(text=f"Editando Plantilla: {name}")
        self.btn_add_template.pack_forget()
        self.btn_save_edit.pack(side=tk.LEFT, padx=2)
        self.btn_cancel_edit.pack(side=tk.LEFT, padx=2)
        
        # Si se cargó desde el popup, cerrarlo para editar
        if self.templates_popup and self.templates_popup.winfo_exists():
            self.templates_popup.destroy()
        
        # Actualizar preview y campos de datos
        self.current_template = (name, template)
        self._refresh_fields_form(template['fields'])
        self._update_preview()
    
    def _save_template_edit(self):
        if not self._editing_template:
            return
        
        fields_str = self.template_fields.get().strip()
        zpl = self.template_zpl.get("1.0", tk.END).strip()
        
        if not fields_str or not zpl:
            messagebox.showwarning("Advertencia", "Completa todos los campos")
            return
        
        fields = [f.strip() for f in fields_str.split(",")]
        self.db.update_template(self._editing_template, zpl, fields)
        messagebox.showinfo("Éxito", f"Plantilla '{self._editing_template}' actualizada")
        self._cancel_edit_template()
    
    def _cancel_edit_template(self):
        self._editing_template = None
        self.form_frame.config(text="Nueva Plantilla")
        
        # Restaurar campo nombre
        self.template_name.config(state="normal")
        self.template_name.delete(0, tk.END)
        self.template_fields.delete(0, tk.END)
        self.template_zpl.delete("1.0", tk.END)
        
        # Restaurar botones
        self.btn_save_edit.pack_forget()
        self.btn_cancel_edit.pack_forget()
        self.btn_add_template.pack(side=tk.LEFT, padx=2)
    
    def _delete_template(self):
        if not self.templates_tree:
            return
            
        selection = self.templates_tree.selection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona una plantilla", parent=self.templates_popup)
            return
            
        item = selection[0]
        name = self.templates_tree.item(item)['values'][0]
        
        # Primero validamos la contraseña del administrador actual
        if self._verify_user_password():
            if messagebox.askyesno("Confirmar", f"¿Estás seguro de eliminar la plantilla '{name}'?\nEsta acción no se puede deshacer.", parent=self.templates_popup):
                self.db.delete_template(name)
                self._refresh_templates_list()
                # Limpiar formulario si era la plantilla cargada
                if self.current_template and self.current_template[0] == name:
                    self.current_template = None
                    self._refresh_fields_form([])

    def _refresh_fields_form(self, fields):
        from src.utils.zpl_utils import RESERVED_KEYWORDS
        for widget in self.fields_frame.winfo_children():
            widget.destroy()
        self.field_entries.clear()
        
        user_fields = [f for f in fields if f not in RESERVED_KEYWORDS]
        
        if not user_fields:
            ttk.Label(self.fields_frame, text="Esta plantilla solo usa palabras reservadas autocompletables.", foreground="gray", font=("Arial", 9, "italic")).pack(pady=10)
            return

        for i, field in enumerate(user_fields):
            ttk.Label(self.fields_frame, text=f"{field}:").grid(row=i, column=0, sticky=tk.W, padx=5, pady=2)
            entry = ttk.Entry(self.fields_frame, width=30)
            entry.grid(row=i, column=1, sticky=tk.EW, padx=5, pady=2)
            self.field_entries[field] = entry
        
        self.fields_frame.columnconfigure(1, weight=1)

    def _save_preset(self):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Carga una plantilla primero")
            return
            
        dialog = tk.Toplevel(self.root)
        dialog.title("Guardar Configuración")
        dialog.geometry("350x150")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Nombre de esta configuración:").pack(pady=10)
        name_entry = ttk.Entry(dialog, width=35)
        name_entry.pack(pady=5)
        name_entry.focus_set()
        
        def save_it():
            name = name_entry.get().strip()
            if not name:
                messagebox.showwarning("Advertencia", "Ingresa un nombre")
                return
            
            data = {field: entry.get() for field, entry in self.field_entries.items()}
            # Incluir configuración de vista previa
            data["_preview_width"] = self.preview_width.get()
            data["_preview_height"] = self.preview_height.get()
            data["_preview_dpmm"] = self.preview_dpmm.get()
            
            self.db.add_label_preset(self.current_template[0], name, data)
            dialog.destroy()
            messagebox.showinfo("Éxito", f"Configuración '{name}' guardada para {self.current_template[0]}")
            if self.presets_popup and self.presets_popup.winfo_exists():
                self._refresh_presets_popup_list()
        
        ttk.Button(dialog, text="Guardar", command=save_it).pack(pady=10)

    def _show_presets_popup(self):
        if not self.current_template:
            messagebox.showwarning("Advertencia", "Carga una plantilla primero")
            return

        if self.presets_popup and self.presets_popup.winfo_exists():
            self.presets_popup.lift()
            return

        self.presets_popup = tk.Toplevel(self.root)
        self.presets_popup.title(f"Configuraciones: {self.current_template[0]}")
        self.presets_popup.geometry("400x500")
        self.presets_popup.transient(self.root)
        
        main_frame = ttk.Frame(self.presets_popup, padding=10)
        main_frame.pack(fill=tk.BOTH, expand=True)

        list_container = ttk.Frame(main_frame)
        list_container.pack(fill=tk.BOTH, expand=True)

        self.presets_listbox = tk.Listbox(list_container, height=15)
        self.presets_listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        scrollbar = ttk.Scrollbar(list_container, orient=tk.VERTICAL, command=self.presets_listbox.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.presets_listbox.config(yscrollcommand=scrollbar.set)
        
        self.presets_listbox.bind("<Double-1>", lambda e: self._load_preset())

        btn_container = ttk.Frame(main_frame)
        btn_container.pack(fill=tk.X, pady=(10, 0))

        ttk.Button(btn_container, text="Cargar", command=self._load_preset).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)
        ttk.Button(btn_container, text="Eliminar", command=self._delete_preset).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)
        ttk.Button(btn_container, text="Cerrar", command=self.presets_popup.destroy).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)

        self._refresh_presets_popup_list()

    def _refresh_presets_popup_list(self):
        if not self.presets_listbox or not self.presets_popup or not self.presets_popup.winfo_exists():
            return
        self.presets_listbox.delete(0, tk.END)
        presets = self.db.get_label_presets(self.current_template[0])
        for p in presets:
            self.presets_listbox.insert(tk.END, p['preset_name'])

    def _load_preset(self):
        selection = self.presets_listbox.curselection()
        if not selection: return
        
        preset_name = self.presets_listbox.get(selection[0])
        presets = self.db.get_label_presets(self.current_template[0])
        preset_data = next((p['data'] for p in presets if p['preset_name'] == preset_name), None)
        
        if preset_data:
            for field, entry in self.field_entries.items():
                entry.delete(0, tk.END)
                if field in preset_data:
                    entry.insert(0, preset_data[field])
            
            if "_preview_width" in preset_data: self.preview_width.set(preset_data["_preview_width"])
            if "_preview_height" in preset_data: self.preview_height.set(preset_data["_preview_height"])
            if "_preview_dpmm" in preset_data: self.preview_dpmm.set(preset_data["_preview_dpmm"])
            
            self._update_preview()

    def _delete_preset(self):
        selection = self.presets_listbox.curselection()
        if not selection: return
        preset_name = self.presets_listbox.get(selection[0])
        if messagebox.askyesno("Borrar", f"¿Eliminar configuración '{preset_name}'?"):
            self.db.delete_label_preset(self.current_template[0], preset_name)
            self._refresh_presets_popup_list()
    
    def _refresh_templates_list(self):
        if not self.templates_tree or not self.templates_popup or not self.templates_popup.winfo_exists():
            return
            
        for item in self.templates_tree.get_children():
            self.templates_tree.delete(item)
        
        templates = self.db.get_templates()
        for t in templates:
            fields = ", ".join(t['fields'])
            self.templates_tree.insert("", "end", values=(t['name'], fields))

    def _show_templates_popup(self):
        if self.templates_popup and self.templates_popup.winfo_exists():
            self.templates_popup.lift()
            return

        self.templates_popup = tk.Toplevel(self.root)
        self.templates_popup.title("Listado de Plantillas")
        self.templates_popup.geometry("600x500")
        self.templates_popup.transient(self.root)
        
        main_popup_frame = ttk.Frame(self.templates_popup, padding=10)
        main_popup_frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main_popup_frame, text="Haz doble clic para editar o usa los botones:", font=("Arial", 10, "bold")).pack(pady=(0, 10))
        
        # Treeview con scrollbar
        list_container = ttk.Frame(main_popup_frame)
        list_container.pack(fill=tk.BOTH, expand=True)

        columns = ("Nombre", "Variables")
        self.templates_tree = ttk.Treeview(list_container, columns=columns, show="headings", height=15)
        self.templates_tree.heading("Nombre", text="Nombre")
        self.templates_tree.heading("Variables", text="Variables")
        self.templates_tree.column("Nombre", width=150)
        self.templates_tree.column("Variables", width=350)
        self.templates_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        scrollbar = ttk.Scrollbar(list_container, orient=tk.VERTICAL, command=self.templates_tree.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.templates_tree.config(yscrollcommand=scrollbar.set)
        
        self.templates_tree.bind("<Double-1>", lambda e: self._load_template_for_edit())

        btn_container = ttk.Frame(main_popup_frame)
        btn_container.pack(fill=tk.X, pady=(10, 0))

        ttk.Button(btn_container, text="Editar Selección", command=self._load_template_for_edit).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)
        ttk.Button(btn_container, text="Eliminar", command=self._delete_template).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)
        ttk.Button(btn_container, text="Cerrar", command=self.templates_popup.destroy).pack(side=tk.LEFT, padx=5, expand=True, fill=tk.X)

        self._refresh_templates_list()
    
    def _add_printer(self):
        name = self.printer_name.get()
        ip = self.printer_ip.get()
        port = int(self.printer_port.get())
        desc = self.printer_desc.get()
        
        if not name or not ip:
            messagebox.showwarning("Advertencia", "Ingresa nombre e IP")
            return
        
        try:
            self.db.add_printer(name, ip, port, desc)
            messagebox.showinfo("Éxito", "Impresora agregada")
            self.printer_name.delete(0, tk.END)
            self.printer_ip.delete(0, tk.END)
            self.printer_desc.delete(0, tk.END)
            self._refresh_printers_list()
        except Exception as e:
            messagebox.showerror("Error", f"Error al agregar impresora: {e}")
    
    def _load_printer_for_edit(self):
        selection = self.printers_tree_main.selection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona una impresora para editar")
            return
        
        item = selection[0]
        values = self.printers_tree_main.item(item)['values']
        name, ip, port, desc = values[0], values[1], values[2], values[3]
        
        # Rellenar el formulario
        self.printer_name.config(state="normal")
        self.printer_name.delete(0, tk.END)
        self.printer_name.insert(0, name)
        self.printer_name.config(state="readonly")   # El nombre es la PK, no se puede cambiar
        
        self.printer_ip.delete(0, tk.END)
        self.printer_ip.insert(0, ip)
        
        self.printer_port.delete(0, tk.END)
        self.printer_port.insert(0, str(port))
        
        self.printer_desc.delete(0, tk.END)
        self.printer_desc.insert(0, desc if desc else "")
        
        # Cambiar modo del formulario a "Editar"
        self._editing_printer = name
        self.printer_form_frame.config(text=f"Editando Impresora: {name}")
        self.btn_add_printer.pack_forget()
        self.btn_save_printer_edit.pack(side=tk.LEFT, padx=2)
        self.btn_cancel_printer_edit.pack(side=tk.LEFT, padx=2)
    
    def _save_printer_edit(self):
        if not self._editing_printer:
            return
        
        ip = self.printer_ip.get().strip()
        desc = self.printer_desc.get().strip()
        
        if not ip:
            messagebox.showwarning("Advertencia", "Ingresa una IP válida")
            return
        
        try:
            port = int(self.printer_port.get())
        except ValueError:
            messagebox.showwarning("Advertencia", "El puerto debe ser un número válido")
            return
        
        self.db.update_printer(self._editing_printer, ip, port, desc)
        messagebox.showinfo("Éxito", f"Impresora '{self._editing_printer}' actualizada")
        self._cancel_edit_printer()
        self._refresh_printers_list()
    
    def _cancel_edit_printer(self):
        self._editing_printer = None
        self.printer_form_frame.config(text="Nueva Impresora")
        
        # Restaurar campo nombre y limpiar formulario
        self.printer_name.config(state="normal")
        self.printer_name.delete(0, tk.END)
        self.printer_ip.delete(0, tk.END)
        self.printer_port.delete(0, tk.END)
        self.printer_port.insert(0, "9100")
        self.printer_desc.delete(0, tk.END)
        
        # Restaurar botones
        self.btn_save_printer_edit.pack_forget()
        self.btn_cancel_printer_edit.pack_forget()
        self.btn_add_printer.pack(side=tk.LEFT, padx=2)
    
    def _delete_printer(self):
        selection = self.printers_tree_main.selection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona una impresora")
            return
        
        item = selection[0]
        name = self.printers_tree_main.item(item)['values'][0]
        
        if messagebox.askyesno("Confirmar", f"¿Eliminar la impresora '{name}'?"):
            self.db.delete_printer(name)
            self._refresh_printers_list()
    
    def _refresh_printers_list(self):
        for item in self.printers_tree_main.get_children():
            self.printers_tree_main.delete(item)
        
        printers = self.db.get_printers()
        for p in printers:
            self.printers_tree_main.insert(
                "",
                "end",
                values=(p['name'], p['ip_address'], p['port'], p['description'])
            )

    def _update_preview(self):
        zpl = self.template_zpl.get("1.0", tk.END).strip()
        if not zpl:
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
            
        img = render_zpl_image(zpl, width=w_inch, height=h_inch, dpmm=dpmm)
        if img:
            # Obtener tamaño del canvas
            canvas_w = self.preview_canvas.winfo_width()
            canvas_h = self.preview_canvas.winfo_height()
            
            if canvas_w < 10: canvas_w = 600
            if canvas_h < 10: canvas_h = 400
            
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
            messagebox.showerror("Error", "No se pudo generar la vista previa. Revisa el código ZPL.")

    def _create_users_tab(self, parent):
        # Formulario
        form_frame = ttk.LabelFrame(parent, text="Nuevo Usuario", padding=10)
        form_frame.pack(fill=tk.X, padx=10, pady=10)

        ttk.Label(form_frame, text="Username / Nomina:").grid(row=0, column=0, sticky=tk.W, padx=5, pady=5)
        self.new_user_name = ttk.Entry(form_frame, width=30)
        self.new_user_name.grid(row=0, column=1, sticky=tk.W, padx=5, pady=5)

        ttk.Label(form_frame, text="Contraseña:").grid(row=0, column=2, sticky=tk.W, padx=5, pady=5)
        self.new_user_pass = ttk.Entry(form_frame, width=20, show="*")
        self.new_user_pass.grid(row=0, column=3, sticky=tk.W, padx=5, pady=5)

        ttk.Label(form_frame, text="Rol:").grid(row=1, column=0, sticky=tk.W, padx=5, pady=5)
        self.new_user_role = ttk.Combobox(form_frame, values=["user", "admin"], state="readonly", width=10)
        self.new_user_role.set("user")
        self.new_user_role.grid(row=1, column=1, sticky=tk.W, padx=5, pady=5)

        ttk.Button(form_frame, text="Crear Usuario", command=self._add_user).grid(row=1, column=3, sticky=tk.E, padx=5, pady=5)

        # Lista de usuarios
        list_frame = ttk.LabelFrame(parent, text="Usuarios del Sistema", padding=10)
        list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        columns = ("ID", "Usuario", "Rol")
        self.users_tree = ttk.Treeview(list_frame, columns=columns, show="headings", height=10)
        self.users_tree.heading("ID", text="ID")
        self.users_tree.heading("Usuario", text="Usuario / Nomina")
        self.users_tree.heading("Rol", text="Permisos")
        
        self.users_tree.column("ID", width=50, anchor=tk.CENTER)
        self.users_tree.column("Usuario", width=200, anchor=tk.W)
        self.users_tree.column("Rol", width=100, anchor=tk.CENTER)
        self.users_tree.pack(fill=tk.BOTH, expand=True)

        ttk.Button(list_frame, text="Eliminar Usuario Seleccionado", command=self._delete_user).pack(pady=10)

        self._refresh_users_list()

    def _add_user(self):
        name = self.new_user_name.get().strip()
        password = self.new_user_pass.get().strip()
        role = self.new_user_role.get()

        if not name or not password:
            messagebox.showwarning("Advertencia", "Completa nombre y contraseña")
            return

        if self.db.add_user(name, password, role):
            messagebox.showinfo("Éxito", f"Usuario '{name}' creado correctamente")
            self.new_user_name.delete(0, tk.END)
            self.new_user_pass.delete(0, tk.END)
            self._refresh_users_list()
        else:
            messagebox.showerror("Error", "El nombre de usuario ya existe")

    def _delete_user(self):
        selection = self.users_tree.selection()
        if not selection:
            messagebox.showwarning("Advertencia", "Selecciona un usuario")
            return

        item = selection[0]
        user_id = self.users_tree.item(item)['values'][0]
        username = self.users_tree.item(item)['values'][1]

        if username == "Admin":
            messagebox.showwarning("Error", "No puedes eliminar al administrador principal")
            return

        if messagebox.askyesno("Confirmar", f"¿Eliminar al usuario '{username}'?"):
            self.db.delete_user(user_id)
            self._refresh_users_list()

    def _refresh_users_list(self):
        for item in self.users_tree.get_children():
            self.users_tree.delete(item)
        
        for u in self.db.get_users():
            self.users_tree.insert("", "end", values=(u['id'], u['username'], u['role']))

    def _create_history_tab(self, parent):
        filter_frame = ttk.LabelFrame(parent, text="Filtros y Búsqueda", padding=10)
        filter_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(filter_frame, text="Plantilla:").pack(side=tk.LEFT, padx=5)
        self.history_filter_var = tk.StringVar(value="Todas")
        templates = ["Todas"] + [t['name'] for t in self.db.get_templates()]
        self.history_filter_combo = ttk.Combobox(
            filter_frame, 
            textvariable=self.history_filter_var, 
            values=templates, 
            state="readonly",
            width=20
        )
        self.history_filter_combo.pack(side=tk.LEFT, padx=5)
        self.history_filter_combo.bind("<<ComboboxSelected>>", lambda e: self._refresh_admin_history())
        
        ttk.Separator(filter_frame, orient=tk.VERTICAL).pack(side=tk.LEFT, padx=15, fill=tk.Y)
        
        ttk.Label(filter_frame, text="Buscar por:").pack(side=tk.LEFT, padx=5)
        self.search_type_var = tk.StringVar(value="Serial")
        self.search_type_combo = ttk.Combobox(
            filter_frame, 
            textvariable=self.search_type_var, 
            values=["Serial", "Material"], 
            state="readonly",
            width=10
        )
        self.search_type_combo.pack(side=tk.LEFT, padx=5)
        
        self.search_value_var = tk.StringVar()
        self.search_entry = ttk.Entry(filter_frame, textvariable=self.search_value_var, width=30)
        self.search_entry.pack(side=tk.LEFT, padx=5)
        self.search_entry.bind("<Return>", lambda e: self._refresh_admin_history())
        
        ttk.Button(filter_frame, text="🔍 Buscar", command=self._refresh_admin_history).pack(side=tk.LEFT, padx=10)
        ttk.Button(filter_frame, text="🔄 Refrescar", command=self._refresh_admin_history).pack(side=tk.LEFT, padx=5)
        ttk.Button(filter_frame, text="📊 Exportar Excel", command=self._export_to_excel).pack(side=tk.LEFT, padx=5)
        
        # Tabla de historial
        list_frame = ttk.LabelFrame(parent, text="Registros de Impresión", padding=10)
        list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        columns = ("Fecha", "Plantilla", "Usuario", "Serial", "Estatus")
        self.admin_history_tree = ttk.Treeview(list_frame, columns=columns, show="headings")
        self.admin_history_tree.heading("Fecha", text="Fecha")
        self.admin_history_tree.heading("Plantilla", text="Plantilla")
        self.admin_history_tree.heading("Usuario", text="Usuario")
        self.admin_history_tree.heading("Serial", text="Serial")
        self.admin_history_tree.heading("Estatus", text="Estatus")
        
        self.admin_history_tree.column("Fecha", width=150)
        self.admin_history_tree.column("Plantilla", width=150)
        self.admin_history_tree.column("Usuario", width=100)
        self.admin_history_tree.column("Serial", width=250)
        self.admin_history_tree.column("Estatus", width=100)
        
        self.admin_history_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # Menú contextual para copiar
        self.admin_history_menu = tk.Menu(self.root, tearoff=0)
        self.admin_history_menu.add_command(label="Copiar Serial", command=lambda: self._copy_admin_history_value(3))
        self.admin_history_menu.add_command(label="Copiar Usuario", command=lambda: self._copy_admin_history_value(2))
        self.admin_history_tree.bind("<Button-3>", self._show_admin_history_context_menu)
        
        # Evento de doble clic para mostrar detalles
        self.admin_history_tree.bind("<Double-1>", self._show_admin_record_details)
        
        scrollbar = ttk.Scrollbar(list_frame, orient=tk.VERTICAL, command=self.admin_history_tree.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.admin_history_tree.config(yscrollcommand=scrollbar.set)
        
        self._refresh_admin_history()

    def _refresh_admin_history(self):
        if not hasattr(self, 'admin_history_tree'):
            return
            
        for item in self.admin_history_tree.get_children():
            self.admin_history_tree.delete(item)
            
        filter_template = self.history_filter_var.get()
        if filter_template == "Todas":
            filter_template = None
            
        search_type = self.search_type_var.get()
        search_value = self.search_value_var.get().strip()
        if not search_value:
            search_type = None
            
        history = self.db.get_print_history(filter_template, search_type, search_value)
        self.admin_history_details = {}
        
        for record in history:
            serials_raw = record['serials']
            if serials_raw and serials_raw.startswith('{') and serials_raw.endswith('}'):
                serials_raw = serials_raw[1:-1]
            
            if serials_raw:
                serial_list = [s.strip() for s in serials_raw.split(',')]
                for sn in serial_list:
                    item_id = self.admin_history_tree.insert("", "end", values=(
                        record['timestamp'],
                        record['template_name'],
                        record['printed_by'] if record['printed_by'] else "N/A",
                        sn,
                        record['status']
                    ))
                    self.admin_history_details[item_id] = (record, sn)
            else:
                item_id = self.admin_history_tree.insert("", "end", values=(
                    record['timestamp'],
                    record['template_name'],
                    record['printed_by'] if record['printed_by'] else "N/A",
                    "N/A",
                    record['status']
                ))
                self.admin_history_details[item_id] = (record, "N/A")
        
    def _export_to_excel(self):
        # Pedir nombre de archivo
        file_path = filedialog.asksaveasfilename(
            defaultextension=".xlsx",
            filetypes=[("Excel files", "*.xlsx")],
            title="Guardar Historial como Excel"
        )
        
        if not file_path:
            return
            
        try:
            # Obtener datos de la base de datos (según los filtros actuales)
            filter_template = self.history_filter_var.get()
            if filter_template == "Todas":
                filter_template = None
                
            search_type = self.search_type_var.get()
            search_value = self.search_value_var.get().strip()
            if not search_value:
                search_type = None
                
            history = self.db.get_print_history(filter_template, search_type, search_value)
            
            # Aplanar los datos para el Excel
            export_data = []
            for record in history:
                # Obtener seriales escaneados para este trabajo
                job_id = record.get('id')
                scanned_serials = self.db.get_scanned_serials_by_job_id(job_id)
                
                # Obtener el serial maestro (limpiando formato {SN})
                master_serial_raw = record['serials']
                if master_serial_raw and master_serial_raw.startswith('{') and master_serial_raw.endswith('}'):
                    master_serial_raw = master_serial_raw[1:-1]
                
                master_serials = [s.strip() for s in master_serial_raw.split(',')] if master_serial_raw else ["N/A"]
                
                # Si hay múltiples seriales maestros (poco común en esta app pero posible), iteramos
                for ms in master_serials:
                    if scanned_serials:
                        for ss in scanned_serials:
                            export_data.append({
                                "Fecha de Impresión": record['timestamp'],
                                "Plantilla": record['template_name'],
                                "Usuario": record['printed_by'] if record['printed_by'] else "N/A",
                                "Serial Maestro": ms,
                                "Serial Escaneado (Material)": ss,
                                "Estatus": record['status']
                            })
                    else:
                        # Si no hay escaneados vinculados
                        export_data.append({
                            "Fecha de Impresión": record['timestamp'],
                            "Plantilla": record['template_name'],
                            "Usuario": record['printed_by'] if record['printed_by'] else "N/A",
                            "Serial Maestro": ms,
                            "Serial Escaneado (Material)": "N/A",
                            "Estatus": record['status']
                        })
            
            if not export_data:
                messagebox.showwarning("Exportar", "No hay datos para exportar con los filtros actuales.")
                return
                
            # Crear DataFrame y guardar a Excel
            df = pd.DataFrame(export_data)
            df.to_excel(file_path, index=False)
            
            # Opcional: una confirmación rápida o simplemente silencio según la nueva política "quiet"
            # Pero en Admin es útil saber que se guardó.
            messagebox.showinfo("Éxito", f"Historial exportado correctamente a:\n{file_path}")
            
            # NUEVO: Preguntar si desea vaciar el historial
            # Basado en la selección de plantilla actual (si es Todas, vacía todo)
            if messagebox.askyesno("Vaciar Historial", "¿Deseas vaciar los registros del historial exportado para iniciar un proceso limpio?"):
                filter_template = self.history_filter_var.get()
                template_to_clear = None if filter_template == "Todas" else filter_template
                self.db.clear_history(template_to_clear)
                self._refresh_admin_history()
            
        except Exception as e:
            messagebox.showerror("Error", f"No se pudo exportar a Excel:\n{str(e)}")

    def _show_admin_record_details(self, event):
        selection = self.admin_history_tree.selection()
        if not selection:
            return
            
        item_id = selection[0]
        if not hasattr(self, 'admin_history_details') or item_id not in self.admin_history_details:
            return
            
        record, sn = self.admin_history_details[item_id]
        
        details_win = tk.Toplevel(self.root)
        details_win.title(f"Detalles - {sn}")
        details_win.geometry("500x400")
        details_win.transient(self.root)
        details_win.grab_set()
        
        import re
        main_frame = ttk.Frame(details_win, padding=10)
        main_frame.pack(fill=tk.BOTH, expand=True)

        ttk.Label(main_frame, text=f"Detalles para Serial: {sn}", font=("Arial", 12, "bold")).pack(pady=(0, 10))
        ttk.Label(main_frame, text=f"Plantilla: {record['template_name']}").pack(anchor=tk.W)
        ttk.Label(main_frame, text=f"Fecha Impresión: {record['timestamp']}").pack(anchor=tk.W, pady=(0, 10))
        
        # Lista de seriales escaneados
        ttk.Label(main_frame, text="Seriales Escaneados Relacionados (Materiales):").pack(anchor=tk.W, pady=(5, 2))
        scanned_listbox = tk.Listbox(main_frame, height=10)
        scanned_listbox.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        if sn != "N/A":
            job_id = record.get('id')
            scanned_serials = self.db.get_scanned_serials_by_job_id(job_id) if job_id else self.db.get_scanned_serials_by_master_serial(sn)
            for code in scanned_serials:
                clean_code = re.sub(r'[^A-Za-z0-9]', '', code)
                if clean_code:
                    scanned_listbox.insert(tk.END, clean_code)
            if not scanned_serials:
                scanned_listbox.insert(tk.END, "Ningún material asociado.")
        else:
            scanned_listbox.insert(tk.END, "Serial Maestro no disponible.")

        # Botón cerrar
        ttk.Button(main_frame, text="Cerrar", command=details_win.destroy).pack(side=tk.RIGHT, pady=10)

    def _show_admin_history_context_menu(self, event):
        item = self.admin_history_tree.identify_row(event.y)
        if item:
            self.admin_history_tree.selection_set(item)
            self.admin_history_menu.post(event.x_root, event.y_root)

    def _copy_admin_history_value(self, index):
        selection = self.admin_history_tree.selection()
        if not selection:
            return
        item = selection[0]
        values = self.admin_history_tree.item(item)['values']
        if values and index < len(values):
            val = values[index]
            self.root.clipboard_clear()
            self.root.clipboard_append(str(val))
            # messagebox.showinfo("Copiado", f"Valor '{val}' copiado al portapapeles")

    def _logout(self):
        if messagebox.askyesno("Cerrar sesión", "¿Deseas cerrar la sesión actual?"):
            self.logout_requested = True
            self.root.destroy()

// Consolidated DOMContentLoaded
document.addEventListener('DOMContentLoaded', function () {
    let currentPartId = null;
    let searchTimeout = null;
    let selectedDate = null;

    const searchInput = document.getElementById("partSearchInput");
    const searchResults = document.getElementById("partSearchResults");
    const partNoDisplay = document.getElementById("currentPartNoDisplay");
    const modal = document.getElementById("manualLogModal");
    const bannerTotalElement = document.getElementById("bannerTotal");

    // Initialize Calendar
    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        dateClick: function(info) {
            console.log('Clicked on: ' + info.dateStr);
            openModal(info.dateStr);
        }
    });
    calendar.render();

    // Function to search parts
    async function searchParts(query) {
        try {
            const response = await fetch("/api/inventory/partno/get/list", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ query: query })
            });
            const data = await response.json();
            renderSearchResults(data.parts || []);
        } catch (error) {
            console.error("Search Error:", error);
        }
    }

    // Function to render results
    function renderSearchResults(parts) {
        searchResults.innerHTML = ""; // Clear current
        if (parts.length === 0) {
            searchResults.innerHTML = '<div class="px-4 py-3 text-xs text-on-surface-variant">No parts found</div>';
        } else {
            parts.forEach(part => {
                const item = document.createElement("div");
                item.className = "px-4 py-3 hover:bg-primary/10 cursor-pointer border-b border-outline-variant/10 last:border-0 transition-colors";
                item.innerHTML = `
                    <div class="text-xs font-bold text-on-surface">${part.part_no}</div>
                    <div class="text-[10px] text-on-surface-variant truncate">${part.description}</div>
                `;
                item.addEventListener("click", () => {
                    console.log("Result clicked:", part.part_no);
                    window.selectPart(part.id, part.part_no);
                });
                searchResults.appendChild(item);
            });
        }
        searchResults.classList.remove("hidden");
    }

    // Selection logic made global for onclick accessibility
    window.selectPart = async (id, partNo) => {
        console.log(`Starting selectPart for: ${partNo} (#${id})`);
        currentPartId = id;
        partNoDisplay.textContent = partNo;
        searchResults.classList.add("hidden");
        searchInput.value = "";
        
        // Fetch full part info
        try {
            console.log("Fetching part info from API...");
            const response = await fetch("/api/inventory/partno/get/info", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ part_id: id })
            });
            
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            
            const data = await response.json();
            console.log("Part Details received:", data.part);
            
            if (data.part) {
                const banner = document.getElementById("partInfoBanner");
                if (banner) {
                    console.log("Updating banner UI...");
                    banner.classList.remove("hidden");
                    
                    document.getElementById("bannerPartNo").textContent = data.part.part_no;
                    document.getElementById("bannerDesc").textContent = data.part.description;
                    document.getElementById("bannerTotal").textContent = Number(data.part.total).toLocaleString('en-US');
                    document.getElementById("bannerProvider").textContent = data.part.provider_name || "N/A";
                    document.getElementById("bannerStockLimitInput").value = data.part.stock_limit ?? 0;
                } else {
                    console.error("Banner element NOT FOUND in DOM");
                }
            }
        } catch (error) {
            console.error("Info Error:", error);
        }

        // Fetch and show calendar events
        fetchAndRenderEvents(id);
    };

    // Save stock limit button
    document.getElementById("bannerStockLimitSaveBtn").addEventListener("click", async () => {
        if (!currentPartId) return;
        const limitVal = parseInt(document.getElementById("bannerStockLimitInput").value, 10);
        if (isNaN(limitVal)) return;
        try {
            const res = await fetch("/api/inventory/partno/update/limit", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ part_id: currentPartId, stock_limit: limitVal })
            });
            const result = await res.json();
            if (result.success) {
                // Refresh calendar so risk indicators recalculate with new limit
                fetchAndRenderEvents(currentPartId);
            } else {
                console.error("Failed to update stock limit:", result.message);
            }
        } catch (err) {
            console.error("Error updating stock limit:", err);
        }
    });

    /**
     * Fetch all history for a part and render on the calendar
     */
    async function fetchAndRenderEvents(partId) {
        if (!partId) return;
        
        try {
            const response = await fetch("/api/inventory/get/all-status", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ part_id: partId })
            });
            
            if (!response.ok) throw new Error("Failed to fetch history");

            const data = await response.json();
            const history = data.history || [];

            // Color mapping for different process types
            const typeConfig = {
                "STATUS QUANTITY": { label: "STOCK", color: "#565e74" }, // Primary
                "REQUIRED QUANTITY": { label: "REQ", color: "#fd4e4d" }, // Risk
                "INCOMING DELIVERY": { label: "DELIV", color: "#006d4a" }, // Secondary
                "ON THE WAY": { label: "OTW", color: "#ba1b24" } // Tertiary
            };

            // Remove existing events
            calendar.removeAllEvents();

            // Add new events
            history.forEach(record => {
                const config = typeConfig[record.name] || { label: record.name, color: "#333" };
                calendar.addEvent({
                    title: `${config.label}: ${Number(record.quantity).toLocaleString()}`,
                    start: record.entry_date,
                    backgroundColor: config.color,
                    borderColor: config.color,
                    allDay: true
                });
            });
            
            // Add risk indicator event if present
            if (data.risk) {
                const color = data.risk.color === 'red' ? '#ff000030' : '#fac80030';
                calendar.addEvent({
                    title: 'STOCKOUT RISK',
                    start: data.risk.date,
                    display: 'background',
                    backgroundColor: color,
                    allDay: true
                });
            }

        } catch (error) {
            console.error("Error rendering calendar events:", error);
        }
    }

    // Event Listeners
    searchInput.addEventListener("input", (e) => {
        const query = e.target.value.trim();
        clearTimeout(searchTimeout);
        if (query.length < 2) {
            searchResults.classList.add("hidden");
            return;
        }
        searchTimeout = setTimeout(() => searchParts(query), 300);
    });

    // Close results when clicking outside
    document.addEventListener("click", (e) => {
        if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
            searchResults.classList.add("hidden");
        }
    });

    async function openModal(dateStr = null) {
        if (!currentPartId) {
            alert("Please search and select a Part Number first.");
            return;
        }

        const today = new Date().toLocaleDateString('sv');
        selectedDate = dateStr || today;
        console.log(`Opening modal for date: ${selectedDate} (Today is: ${today})`);

        // Update modal date display
        const dateEl = document.getElementById("modalCurrentDate");
        if (dateEl) {
            const dateObj = new Date(selectedDate + 'T00:00:00'); // Ensure local time
            const options = { month: 'long', day: 'numeric', year: 'numeric' };
            dateEl.textContent = dateObj.toLocaleDateString('en-US', options);
        }

        const finalStockInput = document.getElementById("input-final-stock");
        const finalStockEditBtn = finalStockInput.nextElementSibling; // The edit button
        const inventoryHeader = document.getElementById("inventory-section-header");
        const inventoryLabel = document.getElementById("inventory-field-label");

        // Logic for Final Stock Field based on Date
        let isPast = selectedDate < today;
        let isFuture = selectedDate > today;
        let isToday = selectedDate === today;

        try {
            const response = await fetch("/api/inventory/get/daily-status", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ part_id: currentPartId, entry_date: selectedDate })
            });
            
            if (!response.ok) {
                console.warn(`Fetch error: ${response.status}`);
                return;
            }

            const data = await response.json();
            
            if (data.status) {
                const masterTotal = bannerTotalElement ? bannerTotalElement.textContent.replace(/,/g, '') : "0";
                
                if (isPast) {
                    // Past date: Must have record in supply_chain_data or show ERROR
                    inventoryHeader.textContent = "Inventory History";
                    inventoryLabel.textContent = "HISTORICAL INVENTORY STATUS";
                    finalStockInput.value = data.status["STATUS QUANTITY"] ?? "ERROR";
                    finalStockInput.setAttribute("readonly", "readonly");
                    if (finalStockEditBtn) finalStockEditBtn.classList.add("hidden");
                } else if (isFuture) {
                    // Future date: Show master stock, read-only
                    inventoryHeader.textContent = "Inventory Update";
                    inventoryLabel.textContent = "CALCULATED INVENTORY QTY";
                    finalStockInput.value = data.status["STATUS QUANTITY"] ?? masterTotal;
                    finalStockInput.setAttribute("readonly", "readonly");
                    if (finalStockEditBtn) finalStockEditBtn.classList.add("hidden");
                } else {
                    // Today: Show SCD quantity or master total fallback, editable
                    inventoryHeader.textContent = "Inventory Update";
                    inventoryLabel.textContent = "Final Stock (Unit Count)";
                    finalStockInput.value = data.status["STATUS QUANTITY"] ?? masterTotal;
                    finalStockInput.setAttribute("readonly", "readonly"); // Starts readonly, edit button enables it
                    if (finalStockEditBtn) finalStockEditBtn.classList.remove("hidden");
                }

                document.getElementById("input-requested-qty").value = data.status["REQUIRED QUANTITY"] ?? 0;
                document.getElementById("input-incoming-delivery").value = data.status["INCOMING DELIVERY"] ?? 0;
                document.getElementById("input-on-the-way").value = data.status["ON THE WAY"] ?? 0;
            }
        } catch (error) {
            console.error("Error loading daily status:", error);
            if (isPast) finalStockInput.value = "ERROR";
        }

        modal.classList.remove("translate-x-full");
        modal.classList.add("translate-x-0");
    }

    function closeModal() {
        modal.classList.add("translate-x-full");
        modal.classList.remove("translate-x-0");
    }

    const btnOpen = document.getElementById("manualLogBtn");
    const btnRecalculate = document.getElementById("recalculateBtn");
    const btnClose1 = document.getElementById("closeModalBtn");
    const btnClose2 = document.getElementById("cancelModalBtn");

    if (btnRecalculate) {
        btnRecalculate.addEventListener("click", async () => {
            if (!currentPartId) {
                alert("Please select a Part Number first.");
                return;
            }

            if (!confirm("This will overwrite individual 'STATUS QUANTITY' records to match inflows/outflows. Proceed?")) {
                return;
            }

            // UI Loading State
            const originalHTML = btnRecalculate.innerHTML;
            btnRecalculate.disabled = true;
            btnRecalculate.innerHTML = '<span class="material-symbols-outlined text-[18px] animate-spin">sync</span> Processing...';
            btnRecalculate.classList.add("opacity-50", "cursor-wait");

            try {
                const response = await fetch("/api/inventory/recalculate", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ part_id: currentPartId })
                });

                const result = await response.json();
                if (result.status === "success") {
                    console.log("Recalculation successful");
                    // Refresh Banner and Inventory Info
                    const currentPartNo = partNoDisplay.textContent;
                    await window.selectPart(currentPartId, currentPartNo);
                    // fetchAndRenderEvents is called inside selectPart
                    alert("Inventory levels recalculated successfully.");
                } else {
                    alert("Error recalculating inventory: " + (result.error || "Unknown error"));
                }
            } catch (error) {
                console.error("Recalculate Error:", error);
                alert("System error during recalculation.");
            } finally {
                // Restore UI State
                btnRecalculate.disabled = false;
                btnRecalculate.innerHTML = originalHTML;
                btnRecalculate.classList.remove("opacity-50", "cursor-wait");
            }
        });
    }

    if (btnOpen) btnOpen.addEventListener("click", () => openModal(new Date().toLocaleDateString('sv')));
    if (btnClose1) btnClose1.addEventListener("click", closeModal);
    if (btnClose2) btnClose2.addEventListener("click", closeModal);

    // Helper to call APIs
    async function saveToAPI(inputId, value) {
        const endpoints = {
            "input-final-stock": { url: "/api/inventory/updateinventorystatus", type: "STATUS QUANTITY" },
            "input-requested-qty": { url: "/api/inventory/requestedqty", type: "REQUIRED QUANTITY" },
            "input-incoming-delivery": { url: "/api/inventory/incomingdelivery", type: "INCOMING DELIVERY" },
            "input-on-the-way": { url: "/api/inventory/ontheway", type: "ON THE WAY" }
        };

        const config = endpoints[inputId];
        if (!config) return;

        if (!currentPartId) {
            alert("Please search and select a Part Number first.");
            return;
        }

        // Clean value (remove commas, parse to float)
        const numericValue = parseFloat(String(value).replace(/,/g, '')) || 0;
        
        const payload = {
            partno_id: currentPartId,
            process_type_name: config.type,
            entry_date: selectedDate, // Use the date selected in the modal
            quantity: numericValue,
            invoice_id: 0 // Placeholder
        };

        try {
            const response = await fetch(config.url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });
            
            if (!response.ok) {
                const errData = await response.json();
                console.error(`Status API Error (${config.type}):`, errData);
                return;
            }

            const data = await response.json();
            console.log(`API Response (${config.type}):`, data);

            // Refresh calendar to show the update
            fetchAndRenderEvents(currentPartId);
        } catch (error) {
            console.error(`API Error (${config.type}):`, error);
        }
    }

    // Logic for input edit buttons
    const editBtns = document.querySelectorAll(".edit-input-btn");
    editBtns.forEach(btn => {
        const input = btn.previousElementSibling;
        
        btn.addEventListener("click", () => {
            if (input.hasAttribute("readonly")) {
                // Enable editing
                input.removeAttribute("readonly");
                input.focus();
                // Select all text when editing starts, typically good UX
                if (input.type !== "number") {
                    input.setSelectionRange(input.value.length, input.value.length);
                }
                btn.textContent = "check";
                btn.title = "Confirm";
                btn.classList.replace("text-on-surface-variant", "text-secondary");
                btn.classList.replace("hover:text-primary", "hover:text-secondary-dim");
            } else {
                // Confirm and disable editing
                input.setAttribute("readonly", "readonly");
                btn.textContent = "edit";
                btn.title = "Edit";
                btn.classList.replace("text-secondary", "text-on-surface-variant");
                btn.classList.replace("hover:text-secondary-dim", "hover:text-primary");

                // Call the API
                saveToAPI(input.id, input.value);
            }
        });

        // Also allow 'Enter' to confirm
        input.addEventListener("keydown", (e) => {
            if (e.key === "Enter" && !input.hasAttribute("readonly")) {
                e.preventDefault();
                btn.click();
            }
        });
    });

    // --- Reset & Upload Data Logic ---
    const uploadModal = document.getElementById("uploadDataModal");
    const openUploadBtn = document.getElementById("resetDataBtn");
    const closeUploadBtn = document.getElementById("closeUploadModalBtn");
    const startImportBtn = document.getElementById("startImportBtn");
    const uploadFileInput = document.getElementById("uploadFileInput");
    const initialDateInput = document.getElementById("initialTotalDateInput");
    const importBtnText = document.getElementById("importBtnText");
    const importBtnIcon = document.getElementById("importBtnIcon");

    if (openUploadBtn) {
        openUploadBtn.addEventListener("click", () => {
            uploadModal.classList.remove("hidden");
        });
    }

    if (closeUploadBtn) {
        closeUploadBtn.addEventListener("click", () => {
            if (startImportBtn.disabled) return; // Prevent closing while importing
            uploadModal.classList.add("hidden");
        });
    }

    if (startImportBtn) {
        startImportBtn.addEventListener("click", async () => {
            const file = uploadFileInput.files[0];
            const date = initialDateInput.value;

            if (!file) {
                alert("Please select an Excel file.");
                return;
            }
            if (!date) {
                alert("Please select an initial date.");
                return;
            }

            if (!confirm("This will PERMANENTLY DELETE all current records. Are you sure?")) {
                return;
            }

            // UI Loading State - Phase 1: Uploading
            startImportBtn.disabled = true;
            importBtnText.textContent = "Uploading...";
            importBtnIcon.textContent = "cloud_upload";
            importBtnIcon.classList.add("animate-spin");

            const formData = new FormData();
            formData.append("file", file);
            formData.append("initial_date", date);

            try {
                const response = await fetch("/api/inventory/upload", {
                    method: "POST",
                    body: formData
                });

                const result = await response.json();

                if (result.status === "started") {
                    // Phase 2: Polling background progress
                    pollImportStatus();
                } else {
                    alert("Upload Error: " + (result.message || "Unknown error"));
                    resetImportUI();
                }
            } catch (error) {
                console.error("Upload Error:", error);
                alert("System error during upload.");
                resetImportUI();
            }
        });
    }

    function resetImportUI() {
        startImportBtn.disabled = false;
        importBtnText.textContent = "Start Import";
        importBtnIcon.textContent = "publish";
        importBtnIcon.classList.remove("animate-spin");
    }

    async function pollImportStatus() {
        const interval = setInterval(async () => {
            try {
                const response = await fetch("/api/inventory/upload-status");
                const result = await response.json();

                if (result.status === "processing") {
                    importBtnText.textContent = result.message || "Processing...";
                    importBtnIcon.textContent = "sync";
                } else if (result.status === "success") {
                    clearInterval(interval);
                    importBtnText.textContent = "Done!";
                    importBtnIcon.classList.remove("animate-spin");
                    importBtnIcon.textContent = "check_circle";
                    alert("Import completed successfully! Refreshing page...");
                    window.location.reload();
                } else if (result.status === "error") {
                    clearInterval(interval);
                    alert("Import Error: " + result.message);
                    resetImportUI();
                }
            } catch (e) {
                console.error("Polling error:", e);
            }
        }, 2000);
    }
});


// // ==========================================
// // 1. CONFIGURACIÓN Y CLIENTE API
// // ==========================================
// const API_BASE_URL = 'http://localhost:8080'; // Asegúrate de cambiar esto según tu entorno

// /**
//  * Función wrapper genérica para hacer peticiones HTTP
//  * Maneja automáticamente los errores HTTP y el parseo de JSON
//  */
// async function fetchAPI(endpoint, options = {}) {
//     // Configuración por defecto: encabezados para hablar en JSON
//     const defaultHeaders = {
//         'Content-Type': 'application/json',
//         'Accept': 'application/json'
//     };

//     const config = {
//         ...options,
//         headers: {
//             ...defaultHeaders,
//             ...options.headers
//         }
//     };

//     try {
//         const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

//         // Si la respuesta no es 2xx, forzamos un error
//         if (!response.ok) {
//             const errorData = await response.json().catch(() => null);
//             throw new Error((errorData && errorData.detail) || `HTTP Error: ${response.status}`);
//         }

//         // Si el servidor retorna 204 No Content, no intentamos parsear JSON
//         if (response.status === 204) {
//             return null;
//         }

//         return await response.json();
//     } catch (error) {
//         console.error(`[API Error en ${endpoint}]:`, error.message);
//         throw error; // Re-lanzar para que pueda ser manejado por quien ejecutó la llamada
//     }
// }

// // ==========================================
// // 2. TUS DEFINICIONES DE ENDPOINTS
// // ==========================================
// const InventoryAPI = {
//     // GET: Obtener todas las partes
//     getParts: () => fetchAPI('/api/parts', { method: 'GET' }),
    
//     // POST: Insertar un histórico manual
//     saveManualLog: (data) => fetchAPI('/api/supply-chain/log', {
//         method: 'POST',
//         body: JSON.stringify(data)
//     })
// };


// // ==========================================
// // 3. CONSUMO DESDE TU INTERFAZ (UI)
// // ==========================================
// document.addEventListener('DOMContentLoaded', () => {

//     // Ejemplo: función conectada a un botón que carga info de la API
//     async function loadData() {
//         try {
//             // Mostrar un spiner de loading idealmente
//             const parts = await InventoryAPI.getParts();
            
//             console.log("Información recibida:", parts);
//             // Hacer algo visual con 'parts' 
//             // document.getElementById('tu-div').innerHTML = parts[0].name;

//         } catch (error) {
//             alert('Falló el intento de traer la información');
//         }
//     }
    
//     // Llamar la función
//     // loadData();
// });

document.addEventListener('DOMContentLoaded', async function () {
    let dashboardData = { kpis: {}, critical_alerts: [], watchlist: [] };

    const alertsContainer = document.getElementById('critical-alerts-container');
    const watchlistTbody = document.getElementById('watchlist-tbody');
    const kpiTotal = document.getElementById('kpi-total-parts');
    const kpiCritical = document.getElementById('kpi-critical-stockouts');
    const kpiRisk = document.getElementById('kpi-risk-alerts');

    const searchInput = document.getElementById('global-search-input');
    const searchResults = document.getElementById('global-search-results');
    const filterAll = document.getElementById('filter-all-items');
    const filterShortages = document.getElementById('filter-shortages');
    const vendorRankingTbody = document.getElementById('vendor-ranking-tbody');
    
    // Modal Elements
    const partModal = document.getElementById('part-summary-modal');
    const modalPartNo = document.getElementById('modal-part-no');
    const modalDesc = document.getElementById('modal-description');
    const modalVendor = document.getElementById('modal-vendor');
    const modalStatus = document.getElementById('modal-status-badge');
    const modalInventory = document.getElementById('modal-inventory');
    const modalLimit = document.getElementById('modal-limit');
    const modalShortage = document.getElementById('modal-shortage-date');
    const modalViewBtn = document.getElementById('modal-view-details');

    async function loadDashboardData() {
        try {
            const response = await fetch('/api/dashboard/summary');
            const data = await response.json();

            if (data.error) {
                console.error(data.error);
                return;
            }

            dashboardData = data;

            // Render KPIs
            if (kpiTotal) kpiTotal.textContent = data.kpis.total_parts.toLocaleString();
            if (kpiCritical) kpiCritical.textContent = data.kpis.critical_stockouts.toLocaleString();
            if (kpiRisk) kpiRisk.textContent = data.kpis.risk_alerts.toLocaleString();

            // Render Alerts
            renderAlerts(data.critical_alerts);

            // Render Watchlist
            renderWatchlist(data.watchlist_preview || data.watchlist);

            // Render Vendors
            renderVendorRanking(data.vendor_ranking);

        } catch (error) {
            console.error('Error loading dashboard:', error);
        }
    }

    function renderAlerts(alerts) {
        if (!alertsContainer) return;

        if (!alerts || alerts.length === 0) {
            alertsContainer.innerHTML = '<div class="p-8 text-center text-text-muted text-xs italic">No current risks detected</div>';
            return;
        }

        alertsContainer.innerHTML = '';
        // Only show first 5 alerts to keep sidebar clean
        alerts.slice(0, 5).forEach(alert => {
            const dateObj = new Date(alert.risk_date + 'T00:00:00');
            const formattedDate = dateObj.toLocaleDateString('en-US', { day: '2-digit', month: 'short', year: '2-digit' });
            
            const card = document.createElement('div');
            const colorClass = alert.risk_color === 'red' ? 'danger' : 'warning';
            
            // Visual logic for acknowledged alerts
            const isAck = alert.is_acknowledged;
            const borderColor = isAck ? 'border-l-gray-300' : `border-l-${colorClass}`;
            const bgColor = isAck ? 'bg-gray-50/50' : `bg-${colorClass}/[0.01] hover:bg-${colorClass}/[0.03]`;
            const opacity = isAck ? 'opacity-70' : 'cursor-pointer';

            card.className = `p-4 rounded-xl border border-border ${bgColor} transition-all border-l-4 ${borderColor} ${opacity} group`;
            
            if (!isAck) {
                card.onclick = () => openPartSummary(alert);
            }

            card.innerHTML = `
                <div class="flex justify-between items-start mb-4">
                    <div class="flex items-center gap-3">
                        <span class="material-symbols-outlined ${isAck ? 'text-text-muted' : 'text-' + colorClass} text-lg">
                            ${isAck ? 'check_circle' : (alert.risk_color === 'red' ? 'error' : 'warning')}
                        </span>
                        <div>
                            <p class="text-xs font-bold text-text-main">Part No. ${alert.part_no}</p>
                            <p class="text-[10px] text-text-muted uppercase truncate max-w-[150px]">${alert.description}</p>
                        </div>
                    </div>
                    <div class="flex items-center gap-1.5">
                        ${isAck ? 
                            `<span class="text-[9px] font-bold text-text-muted border border-gray-200 px-1.5 py-0.5 rounded uppercase bg-white">Acredited</span>` :
                            `<span class="text-[9px] font-bold text-${colorClass} border border-${colorClass}/20 px-1.5 py-0.5 rounded uppercase">${alert.risk_color === 'red' ? 'Stockout' : 'Risk'}</span>
                             <button onclick="event.stopPropagation(); window.acknowledgeRisk(${alert.id}, '${alert.risk_date}')" 
                                     class="size-6 flex items-center justify-center rounded-full hover:bg-gray-200 text-text-muted transition-colors"
                                     title="Acreditar alerta">
                                <span class="material-symbols-outlined text-[16px]">done</span>
                             </button>`
                        }
                    </div>
                </div>
                <div class="grid grid-cols-2 gap-y-3">
                    <div class="flex flex-col">
                        <p class="text-[9px] text-text-muted uppercase font-semibold">Shortage Date</p>
                        <p class="text-[11px] ${isAck ? 'text-text-muted' : 'text-' + colorClass} font-bold">${formattedDate}</p>
                    </div>
                    <div class="flex flex-col">
                        <p class="text-[9px] text-text-muted uppercase font-semibold">Inventory</p>
                        <p class="text-[11px] text-text-main font-bold">${Number(alert.total || 0).toLocaleString()}</p>
                    </div>
                </div>
            `;
            alertsContainer.appendChild(card);
        });
    }

    window.acknowledgeRisk = async function(partId, date) {
        try {
            const response = await fetch('/api/inventory/acknowledge', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ part_id: partId, until_date: date })
            });
            const result = await response.json();
            if (result.success) {
                // Refresh data to show changes
                await loadDashboardData();
            } else {
                alert('Error al acreditar la alerta');
            }
        } catch (error) {
            console.error('Error acknowledging risk:', error);
        }
    };

    function renderWatchlist(items) {
        if (!watchlistTbody) return;

        watchlistTbody.innerHTML = '';
        if (!items || items.length === 0) {
            watchlistTbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center text-text-muted italic">No items found matching the current filter.</td></tr>';
            return;
        }

        items.forEach(item => {
            const row = document.createElement('tr');
            row.className = 'hover:bg-gray-50/50 transition-colors cursor-pointer';
            row.onclick = () => window.location.href = `/inventory?part_id=${item.id}`;

            const statusLabel = item.risk_color === 'red' ? 'Critical' : (item.risk_color === 'yellow' ? 'Warning' : 'Healthy');
            const statusColor = item.risk_color === 'red' ? 'danger' : (item.risk_color === 'yellow' ? 'warning' : 'primary');
            
            const dateStr = item.risk_date ? new Date(item.risk_date + 'T00:00:00').toLocaleDateString('en-US', { day: '2-digit', month: 'short', year: '2-digit' }) : 'No Shortage';

            row.innerHTML = `
                <td class="px-4 py-4 align-top">
                    <div class="text-text-main font-semibold">${item.part_no}</div>
                </td>
                <td class="px-4 py-4 align-top">
                    <div class="text-text-main font-semibold truncate max-w-[200px]">${item.description}</div>
                    <div class="text-[10px] text-text-muted">${item.provider_name || 'N/A'}</div>
                </td>
                <td class="px-4 py-4 align-top text-right font-medium text-text-main">${Number(item.total || 0).toLocaleString()}</td>
                <td class="px-4 py-4 align-top text-center">
                    <span class="inline-flex items-center text-[10px] font-bold text-${statusColor} px-2 py-0.5 bg-${statusColor}/10 rounded uppercase">${statusLabel}</span>
                </td>
                <td class="px-4 py-4 align-top text-right font-bold text-${statusColor} uppercase">${dateStr}</td>
                <td class="px-4 py-4 align-top text-right">
                    <div class="text-text-main font-medium">—</div>
                </td>
                <td class="px-4 py-4 align-top text-right">
                    <div class="text-text-main font-medium">—</div>
                </td>
                <td class="px-4 py-4 align-top text-right">
                    <div class="text-text-main font-medium">—</div>
                </td>
            `;
            watchlistTbody.appendChild(row);
        });
    }

    // Global Search Logic
    let searchTimeout = null;
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.trim();
            clearTimeout(searchTimeout);
            if (query.length < 2) {
                if (searchResults) searchResults.classList.add('hidden');
                return;
            }
            searchTimeout = setTimeout(() => searchGlobal(query), 300);
        });
    }

    async function searchGlobal(query) {
        try {
            const response = await fetch("/api/inventory/partno/get/list", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ query: query })
            });
            const data = await response.json();
            renderSearchResults(data.parts || []);
        } catch (error) {
            console.error("Global Search Error:", error);
        }
    }

    function renderSearchResults(parts) {
        if (!searchResults) return;
        searchResults.innerHTML = "";
        if (parts.length === 0) {
            searchResults.innerHTML = '<div class="px-4 py-3 text-xs text-text-muted italic">No parts found</div>';
        } else {
            parts.forEach(part => {
                const div = document.createElement('div');
                div.className = "px-4 py-3 hover:bg-primary/10 cursor-pointer border-b border-border last:border-0 transition-colors";
                div.innerHTML = `
                    <div class="text-xs font-bold text-text-main">${part.part_no}</div>
                    <div class="text-[10px] text-text-muted truncate">${part.description}</div>
                `;
                div.onclick = () => window.location.href = `/inventory?part_id=${part.id}`;
                searchResults.appendChild(div);
            });
        }
        searchResults.classList.remove('hidden');
    }

    // Hide search results when clicking outside
    document.addEventListener('click', (e) => {
        if (searchInput && searchResults && !searchInput.contains(e.target) && !searchResults.contains(e.target)) {
            searchResults.classList.add('hidden');
        }
    });

    // Filtering Logic
    if (filterAll) {
        filterAll.addEventListener('click', () => {
            // Reset vendor row highlighting
            document.querySelectorAll('.vendor-row').forEach(r => r.classList.remove('bg-primary/10'));
            setActiveFilter(filterAll, filterShortages);
            renderWatchlist(dashboardData.watchlist_preview || dashboardData.watchlist);
        });
    }

    if (filterShortages) {
        filterShortages.addEventListener('click', () => {
            setActiveFilter(filterShortages, filterAll);
            const shortages = dashboardData.watchlist.filter(item => item.risk_color === 'red' || item.risk_color === 'yellow');
            renderWatchlist(shortages);
        });
    }

    function setActiveFilter(active, inactive) {
        if (active) {
            active.className = "px-3 py-1.5 rounded-lg text-[10px] font-bold bg-primary text-white border border-primary transition-all";
        }
        if (inactive) {
            inactive.className = "px-3 py-1.5 rounded-lg text-[10px] font-bold text-text-muted border border-border hover:bg-gray-50 transition-all";
        }
    }

    function renderVendorRanking(vendors) {
        if (!vendorRankingTbody) return;
        vendorRankingTbody.innerHTML = '';

        if (!vendors || vendors.length === 0) {
            vendorRankingTbody.innerHTML = '<tr><td colspan="2" class="px-4 py-8 text-center text-text-muted italic">No data available</td></tr>';
            return;
        }

        vendors.forEach(vendor => {
            const row = document.createElement('tr');
            row.className = 'hover:bg-primary/5 cursor-pointer transition-colors border-b border-border last:border-0 group vendor-row';
            row.dataset.vendor = vendor.name;
            row.onclick = () => filterByVendor(vendor.name, row);

            row.innerHTML = `
                <td class="px-4 py-3">
                    <div class="text-text-main font-semibold group-hover:text-primary transition-colors vendor-name">${vendor.name}</div>
                    <div class="text-[9px] text-text-muted uppercase tracking-tight">${vendor.total_parts} Total Parts</div>
                </td>
                <td class="px-4 py-3 text-center">
                    <div class="inline-flex items-center gap-1">
                        <span class="size-2 rounded-full bg-danger"></span>
                        <span class="font-bold text-text-main">${vendor.critical}</span>
                    </div>
                </td>
            `;
            vendorRankingTbody.appendChild(row);
        });
    }

    function filterByVendor(vendorName, selectedRow) {
        if (!dashboardData) return;
        
        // Visual feedback for selected vendor
        document.querySelectorAll('.vendor-row').forEach(r => r.classList.remove('bg-primary/10'));
        if (selectedRow) selectedRow.classList.add('bg-primary/10');

        // Deactivate button filters
        setActiveFilter(null, filterAll);
        setActiveFilter(null, filterShortages);

        console.log(`Filtering by vendor: ${vendorName}`);
        const filtered = dashboardData.watchlist.filter(item => item.provider_name === vendorName);
        console.log(`Found ${filtered.length} items`);
        
        renderWatchlist(filtered);
    }

    function openPartSummary(part) {
        if (!partModal) return;

        modalPartNo.textContent = `Part No. ${part.part_no}`;
        modalDesc.textContent = part.description;
        modalVendor.textContent = part.provider_name || 'Unknown';
        modalInventory.textContent = Number(part.total || 0).toLocaleString();
        modalLimit.textContent = Number(part.stock_limit || 0).toLocaleString();
        
        const statusLabel = part.risk_color === 'red' ? 'Critical' : (part.risk_color === 'yellow' ? 'Warning' : 'Healthy');
        const statusColor = part.risk_color === 'red' ? 'danger' : (part.risk_color === 'yellow' ? 'warning' : 'primary');
        modalStatus.innerHTML = `<span class="inline-flex items-center text-[10px] font-bold text-${statusColor} px-2 py-0.5 bg-${statusColor}/10 rounded uppercase">${statusLabel}</span>`;
        
        const dateStr = part.risk_date ? new Date(part.risk_date + 'T00:00:00').toLocaleDateString('en-US', { day: '2-digit', month: 'short', year: '2-digit' }) : 'No Shortage';
        modalShortage.textContent = dateStr;
        modalShortage.className = `text-lg font-bold ${part.risk_color === 'red' ? 'text-danger' : (part.risk_color === 'yellow' ? 'text-warning' : 'text-primary')}`;

        modalViewBtn.onclick = () => window.location.href = `/inventory?part_id=${part.id}`;

        partModal.classList.remove('hidden');
    }

    document.querySelectorAll('.modal-close').forEach(btn => {
        btn.onclick = () => partModal.classList.add('hidden');
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') partModal.classList.add('hidden');
    });

    loadDashboardData();
});

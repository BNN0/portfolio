export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    error?: string;
}

class PrinterService {
    private baseUrl: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    private getHeaders(): HeadersInit {
        return {
            'Content-Type': 'application/json',
        };
    }

    private async handleResponse<T>(response: Response): Promise<ApiResponse<T>> {
        try {
            if (!response.ok) {
                var errormessage = (await response.json())
                throw new Error(`HTTP error! status: ${response.status}. Message: ${errormessage.message}`);
            }

            const data: T = await response.json();
            return {
                success: true,
                data
            };
        } catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error desconocido'
            };
        }
    }

    async healthStatus(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/health`, {
                method: 'GET',
                headers: this.getHeaders(),
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async infoServer(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/get-ip`, {
                method: 'GET',
                headers: this.getHeaders(),
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async listPrinters(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/list-host-printers`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify({"level_info":2})
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async getStoragedPrinters(): Promise<ApiResponse<any>> {
        try {
            console.log(this.baseUrl)
            const response = await fetch(`${this.baseUrl}/stored-printers`, {
                method: 'GET',
                headers: this.getHeaders(),
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async savePrinter(request:any): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/save-printer-config`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async getPrinterStatus(printer_id:any): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/get-printer-status/${printer_id}`, {
                method: 'POST',
                headers: this.getHeaders()
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async testPrinterConnection(request:any): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/print-zpl`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async updatePrinterConfig(request:any): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/update-printer-config`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async deletePrinterById(request:any): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/delete-printer/${request}`, {
                method: 'DELETE',
                headers: this.getHeaders(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response)
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }
    
}


let printerService: PrinterService;

export function setPrinterServiceBaseUrl(baseUrl: string) {
    printerService = new PrinterService(baseUrl);
}

export { printerService };

// export default printerService;
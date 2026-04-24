export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    error?: string;
}

class ServerService {
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
            const response = await fetch(`${this.baseUrl}/status`, {
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

    async startServer(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/start`, {
                method: 'POST',
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

    async stopServer(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/stop`, {
                method: 'POST',
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

    async restartServer(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/restart`, {
                method: 'POST',
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
            const response = await fetch(`${this.baseUrl}/restart`, {
                method: 'POST',
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

    async getBackendAddress(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/backend-address`, {
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

    setBaseUrl(newUrl: string) {
        this.baseUrl = newUrl;
    }
}

const getInitialBaseUrl = () => {
    try {
        const storedConfig = localStorage.getItem('printer_server_config');
        if (storedConfig) {
            const { ip, port } = JSON.parse(storedConfig);
            if (ip && port) {
                return `http://${ip}:${port}`;
            }
        }
    } catch (error) {
        console.error('Error loading server config:', error);
    }
    return 'http://localhost:8091';
};


const serverService = new ServerService(getInitialBaseUrl());

export default serverService;
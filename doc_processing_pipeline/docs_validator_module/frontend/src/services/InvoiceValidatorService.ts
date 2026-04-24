export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    error?: string;
}

export interface ValidationResult {
    validationId: string;
    invoiceNumber: string;
    files: {
        logistic: string[];
        sat: string[];
    };
    fields: FieldComparison[];
    details?: DetailComparison[];
    timestamp: string;
}

export interface FieldComparison {
    campo: string;
    logistica: string;
    sat: string;
    estado: 'success' | 'danger';
}

export interface DetailComparison {
    campo: string;
    logistica: string;
    sat: string;
    estado: 'success' | 'danger';
}

export interface SendObservationsRequest {
    email: string;
    validationId: string;
    invoiceNumber: string;
    fields: FieldComparison[];
}

class InvoiceValidatorService {
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
                const errorMessage = await response.json();
                throw new Error(`HTTP error! status: ${response.status}. Message: ${errorMessage.message}`);
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

    async validateInvoices(
        logisticFiles: File[],
        satFiles: File[]
    ): Promise<ApiResponse<ValidationResult>> {
        try {
            const formData = new FormData();

            // Agregar archivos logísticos
            logisticFiles.forEach((file) => {
                formData.append('logisticFiles', file);
            });

            // Agregar archivos SAT
            satFiles.forEach((file) => {
                formData.append('satFiles', file);
            });

            const response = await fetch(`${this.baseUrl}/validate-invoices`, {
                method: 'POST',
                headers: {
                    'accept': '*/*',
                },
                body: formData
            });

            return await this.handleResponse<ValidationResult>(response);
        } catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            };
        }
    }

    async sendObservations(
        request: SendObservationsRequest
    ): Promise<ApiResponse<{ message: string }>> {
        try {
            const response = await fetch(`${this.baseUrl}/send-observations`, {
                method: 'POST',
                headers: this.getHeaders(),
                body: JSON.stringify(request)
            });

            return await this.handleResponse<{ message: string }>(response);
        } catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            };
        }
    }

    async getValidationHistory(): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/validations`, {
                method: "GET",
                headers: this.getHeaders()
            });

            return await this.handleResponse<any>(response);
        } catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : "Error de conexión"
            };
        }
    }

    async deleteValidation(validationId: string): Promise<ApiResponse<{ message: string }>> {
        try {
            const response = await fetch(`${this.baseUrl}/validations/${validationId}`, {
                method: 'DELETE',
                headers: this.getHeaders()
            });

            return await this.handleResponse<{ message: string }>(response);
        } catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            };
        }
    }

}

const invoiceValidatorService = new InvoiceValidatorService('http://192.168.10.207:8081');

export default invoiceValidatorService;
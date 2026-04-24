export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    error?: string;
}

class BackendServices {
    private baseUrl: string;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    private getHeadersJson(): HeadersInit {
        return {
            'Content-Type': 'application/json',
        };
    }

    private async handleResponse<T>(response: Response): Promise<ApiResponse<T>> {
        try {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
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


    async uploadFiles(request: File[]): Promise<ApiResponse<File[]>> {
        try {
            const formData = new FormData();

            request.forEach(file => {
                formData.append('files', file);
            });


            const response = await fetch(`${this.baseUrl}/api/files/upload-multiple`, {
                method: 'POST',
                headers: {
                    'accept': '*/*'
                },
                body: formData
            });

            return await this.handleResponse<any>(response);
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            };
        }
    }

    async listFiles(bucketName: string): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/api/files/list/${bucketName}`, {
                method: 'GET',
                headers: {
                    'accept': 'application/json',
                }
            });
            return await this.handleResponse<any>(response);
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async downloadFile(objectName: string, bucketName: string): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/api/files/download`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    object_name: objectName,
                    bucket_name: bucketName
                })
            });

            if (response.ok) {
                // Crear un blob y descargarlo
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = objectName;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);

                return { success: true };
            }

            return { success: false, error: 'Error downloading file' };
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async deleteFile(objectName: string, bucketName: string): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/api/files/delete`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    object_name: objectName,
                    bucket_name: bucketName
                })
            });

            if (response.ok) {
                return { success: true };
            }

            return { success: false, error: 'Error deleting file' };
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async processFile(fileName: string, objectName: string, bucketName: string): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(`${this.baseUrl}/api/pdf/process-pdf`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    filename: fileName,
                    object_name: objectName,
                    bucket_name: bucketName
                })
            });

            if (response.ok) {
                return { success: true };
            }

            return { success: false, error: 'Error deleting file' };
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async processFileAsync(fileName: string, objectName: string, bucketName: string): Promise<ApiResponse<any>> {
        try {
            const request = {
                filename: fileName,
                object_name: objectName,
                bucket_name: bucketName
            }
            const response = await fetch(`${this.baseUrl}/api/pdf/process-pdf-async`, {
                method: 'POST',
                headers: this.getHeadersJson(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response);
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async getFileStatus(objectName: string, bucketName: string): Promise<ApiResponse<any>> {
        try {
            const response = await fetch(
                `${this.baseUrl}/api/pdf/status/${objectName}?bucket_name=${bucketName}`
            );
            return await this.handleResponse<any>(response);
        } catch (error) {
            console.error('Error in getFileStatus:', error);
            return { success: false, error: 'Network error' };
        }
    }

    async insertPrompt(prompt_text: string): Promise<ApiResponse<any>> {
        try {
            const request = {
                prompt_text: prompt_text
            }
            const response = await fetch(`${this.baseUrl}/api/ai/prompt`, {
                method: 'POST',
                headers: this.getHeadersJson(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response);
        }
        catch (error) {
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async getPrompt(): Promise<ApiResponse<any>> {
        try{
            const response = await fetch(`${this.baseUrl}/api/ai/prompt`, {
                method: 'GET',
                headers: this.getHeadersJson(),
            });
            return await this.handleResponse<any>(response);
        }
        catch (error){
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }

    async improvePrompt(prompt_text: string): Promise<ApiResponse<any>> {
        try{
            let request = {
                prompt_text : prompt_text
            }

            const response = await fetch(`${this.baseUrl}/api/ai/prompt-enhance`, {
                method: 'POST',
                headers: this.getHeadersJson(),
                body: JSON.stringify(request)
            });
            return await this.handleResponse<any>(response);
        }
        catch(error){
            return {
                success: false,
                error: error instanceof Error ? error.message : 'Error de conexión'
            }
        }
    }
}


const backendServices = new BackendServices('http://localhost:8011');

export default backendServices;
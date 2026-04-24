import { useState, useEffect, useRef } from 'react';
import backendServices from '../services/backendServices';

interface PollingResult {
  status: string;
  hasModified: number;
  isLoading: boolean;
}

const useFileStatusPolling = (
  objectName: string, 
  bucketName: string, 
  isProcessing: boolean,
  initialStatus: string = 'pending'
): PollingResult => {
  const [status, setStatus] = useState<string>(initialStatus);
  const [hasModified, setHasModified] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const pollCountRef = useRef(0);
  const maxPolls = 200; // Máximo 10 minutos (200 * 3 segundos)

  useEffect(() => {
    setStatus(initialStatus);
    setHasModified(0);
    pollCountRef.current = 0;
  }, [objectName, initialStatus]);

  useEffect(() => {
    if (!isProcessing || !objectName) {
      console.log(`Polling stopped: isProcessing=${isProcessing}, objectName=${objectName}`);
      setIsLoading(false);
      return;
    }

    let isMounted = true;
    let intervalId: NodeJS.Timeout | null = null;
    const pollingInterval = 3000;

    const pollStatus = async (): Promise<boolean> => {
      if (!isMounted) return true;

      // Protección contra polling infinito
      pollCountRef.current++;
      if (pollCountRef.current > maxPolls) {
        console.warn(`Polling timeout for ${objectName}`);
        setStatus('error');
        return true;
      }

      try {
        setIsLoading(true);
        const response = await backendServices.getFileStatus(objectName, bucketName);
        
        if (!isMounted) return true;

        if (response.success && response.data) {
          const statusFromAPI = response.data.status || 'pending';
          const hasModifiedFromAPI = response.data.has_modified || 0;
          
          console.log(`Poll #${pollCountRef.current} - Status: ${statusFromAPI}, File: ${objectName}`);
          
          setStatus(statusFromAPI);
          setHasModified(hasModifiedFromAPI);
          
          // Estados finales que detienen el polling
          const finalStatuses = ['fixed', 'no_changes', 'error', 'completed', 'ready'];
          if (finalStatuses.includes(statusFromAPI)) {
            console.log(`Final status reached: ${statusFromAPI} for ${objectName}`);
            return true;
          }
          
          // Estados intermedios continúan polling
          const processingStatuses = ['analyzing', 'issues', 'processing'];
          if (processingStatuses.includes(statusFromAPI)) {
            return false;
          }
        } else {
          console.error('Failed to get status:', response.error);
        }
      } catch (error) {
        console.error('Error polling status:', error);
        if (isMounted) {
          setStatus('error');
          return true;
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
      return false;
    };

    // Iniciar polling con delay inicial para dar tiempo al backend
    const startPolling = async () => {
      // Esperar 2 segundos antes del primer poll
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      if (!isMounted) return;

      const shouldStop = await pollStatus();
      if (shouldStop || !isMounted) return;

      // Configurar intervalo
      intervalId = setInterval(async () => {
        const shouldStop = await pollStatus();
        if (shouldStop && intervalId) {
          clearInterval(intervalId);
          intervalId = null;
        }
      }, pollingInterval);
    };

    startPolling();

    return () => {
      isMounted = false;
      if (intervalId) {
        clearInterval(intervalId);
      }
      setIsLoading(false);
    };
  }, [objectName, bucketName, isProcessing]);

  return { status, hasModified, isLoading };
};

export default useFileStatusPolling;
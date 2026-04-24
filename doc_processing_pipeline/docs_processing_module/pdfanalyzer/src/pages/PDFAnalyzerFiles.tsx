import React, { useState, useEffect } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonButton,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonCard,
  IonCardContent,
  IonGrid,
  IonRow,
  IonCol,
  IonSearchbar,
  IonChip,
  IonAvatar,
  IonBadge,
  IonSpinner,
  IonToast,
  IonRefresher,
  IonRefresherContent,
  IonModal,
  IonTextarea,
  IonFooter,
  IonPopover,
  IonText
} from '@ionic/react';
import {
  scanCircleOutline,
  chevronDownOutline,
  cloudDownloadOutline,
  checkmarkCircle,
  timeOutline,
  warningOutline,
  addOutline,
  refreshOutline,
  closeOutline,
  trashBinOutline,
  searchCircleOutline,
  searchOutline,
  layersOutline,
  fileTrayFullOutline,
  buildOutline,
  enterOutline,
  constructOutline,
  colorWandOutline,
  alertCircleOutline
} from 'ionicons/icons';
import { RefresherEventDetail } from '@ionic/core';
import backendServices from '../services/backendServices';
import { useHistory } from 'react-router';
import FileListItem from '../components/FileListItemComponent'; // Importar el nuevo componente
import './PDFAnalyzerFiles.css'
import Tooltip from '../components/complements/Tooltip';

interface FileData {
  file_name: string;
  object_name: string;
  size_bytes: number;
  last_modified: string;
  etag: string;
  status: string;
  has_modified: number;
}

const PDFAnalyzerFiles: React.FC = () => {
  const [searchText, setSearchText] = useState('');
  const [files, setFiles] = useState<FileData[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [loadingPrompt, setLoadingPrompt] = useState<boolean>(true);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [toastColor, setToastColor] = useState<'success' | 'danger'>('success');
  const [downloadingFile, setDownloadingFile] = useState<string | null>(null);
  const [showPromptModal, setShowPromptModal] = useState(false);
  const [showAIModal, setShowAIModal] = useState(false);
  const [prompt, setPrompt] = useState('');
  const [processingFiles, setProcessingFiles] = useState<Set<string>>(new Set())
  const [fileStatuses, setFileStatuses] = useState<Record<string, string>>({})
  const history = useHistory();

  const BUCKET_ORIGIN_NAME = 'pdf-uploads'; // Bucket por defecto
  const BUCKET_MODIFY_NAME = 'pdf-modified';


  // Cargar archivos al montar el componente
  useEffect(() => {
    loadFiles();
  }, []);
  // Agregar useEffect para limpiar processingFiles cuando el status cambia
  useEffect(() => {
    files.forEach(file => {
      // Si el archivo ya no está en estado de procesamiento, removerlo de processingFiles
      const finalStatuses = ['fixed', 'no_changes', 'error'];
      if (finalStatuses.includes(file.status) && processingFiles.has(file.object_name)) {
        setProcessingFiles(prev => {
          const newSet = new Set(prev);
          newSet.delete(file.object_name);
          return newSet;
        });
      }
    });
  }, [files]);

  const analyzeFile = async (fileName: string, objectName: string) => {
    try {
      // Marcar como procesando ANTES de hacer la llamada
      setProcessingFiles(prev => {
        const newSet = new Set(prev);
        newSet.add(objectName);
        return newSet;
      });

      // Actualizar el estado local del archivo inmediatamente
      setFiles(prevFiles =>
        prevFiles.map(f =>
          f.object_name === objectName
            ? { ...f, status: 'analyzing' }
            : f
        )
      );

      console.log(`Starting analysis for: ${fileName}`);

      const response = await backendServices.processFileAsync(fileName, objectName, BUCKET_ORIGIN_NAME);

      if (!response.success) {
        setToastMessage(`Error starting analysis: ${response.error || 'Unknown error'}`);
        setToastColor('danger');
        setShowToast(true);

        // Revertir estado en caso de error
        setProcessingFiles(prev => {
          const newSet = new Set(prev);
          newSet.delete(objectName);
          return newSet;
        });

        setFiles(prevFiles =>
          prevFiles.map(f =>
            f.object_name === objectName
              ? { ...f, status: 'error' }
              : f
          )
        );
      } else {
        console.log(`Analysis started successfully for: ${fileName}`);
        setToastMessage(`Analysis started: ${fileName}`);
        setToastColor('success');
        setShowToast(true);
      }

    } catch (error) {
      console.error('Error starting file processing:', error);
      setToastMessage(`Connection error: ${error}`);
      setToastColor('danger');
      setShowToast(true);

      setProcessingFiles(prev => {
        const newSet = new Set(prev);
        newSet.delete(objectName);
        return newSet;
      });
    }
  };

  const analyzeAll = async () => {
    try {
      setLoading(true);
      const response = await backendServices.listFiles(BUCKET_ORIGIN_NAME);

      if (response.success) {
        const pendingFiles = response.data.files.filter((file: FileData) =>
          file.status === "pending" && !processingFiles.has(file.object_name)
        );

        if (pendingFiles.length === 0) {
          setToastMessage('No pending files to analyze');
          setToastColor('danger');
          setShowToast(true);
          setLoading(false);
          return;
        }

        setToastMessage(`Starting analysis of ${pendingFiles.length} files...`);
        setToastColor('success');
        setShowToast(true);

        // Procesar en lotes de 3 con delays
        const CONCURRENT_LIMIT = 3;
        const DELAY_BETWEEN_BATCHES = 2000;

        for (let i = 0; i < pendingFiles.length; i += CONCURRENT_LIMIT) {
          const batch = pendingFiles.slice(i, i + CONCURRENT_LIMIT);

          console.log(`Processing batch ${Math.floor(i / CONCURRENT_LIMIT) + 1} of ${Math.ceil(pendingFiles.length / CONCURRENT_LIMIT)}`);

          // Procesar batch en paralelo
          await Promise.allSettled(
            batch.map((file: FileData) => analyzeFile(file.file_name, file.object_name))
          );

          // Delay entre batches (excepto después del último)
          if (i + CONCURRENT_LIMIT < pendingFiles.length) {
            await new Promise(resolve => setTimeout(resolve, DELAY_BETWEEN_BATCHES));
          }
        }

        setToastMessage(`All files submitted for analysis`);
        setToastColor('success');
        setShowToast(true);
      }
    } catch (error) {
      console.error('Error processing files:', error);
      setToastMessage('Connection error while processing files');
      setToastColor('danger');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const sendPrompt = async () => {
    try {
      setLoading(true)
      let text = prompt;
      console.log(text)

      const response = await backendServices.insertPrompt(text);

      if (response.success) {
        const new_prompt = response.data.prompt;
        setPrompt(new_prompt)
        setShowPromptModal(false)
      }
    } catch (error) {
      console.error('Error sending prompt:', error);
      setToastMessage('Connection error while sending prompt');
      setShowToast(true);
    } finally {
      setLoading(false);
    }
  };

  const getPrompt = async () => {
    try {
      setLoadingPrompt(true)
      const response = await backendServices.getPrompt();

      if (response.success) {
        const prompt_text = await response.data.prompt
        setPrompt(prompt_text)
      }
    } catch (error) {
      console.error('Error getting prompt:', error);
      setToastMessage('Connection error while getting prompt');
      setShowToast(true);
    } finally {
      setLoadingPrompt(false)
    }
  }

  const improvePrompt = async () => {
    try {
      setLoadingPrompt(true)

      let prompt_text = prompt;
      const response = await backendServices.improvePrompt(prompt_text);
      if (response.success) {
        let enhance_prompt = await response.data.prompt;
        setPrompt(enhance_prompt)
      }
    } catch (error) {
      console.error('Error improve prompt:', error);
      setToastMessage('Connection error while improving prompt');
      setShowToast(true);
    } finally {
      setLoadingPrompt(false)
    }
  };


  // Optimizar la carga de archivos
  // Mejorar loadFiles para respetar estados de procesamiento
  const loadFiles = async (showLoading: boolean = true) => {
    try {
      if (showLoading) setLoading(true);

      const response = await backendServices.listFiles(BUCKET_ORIGIN_NAME);

      if (response.success) {
        const mappedFiles: FileData[] = response.data.files.map((file: any) => {
          // Mantener estado 'analyzing' si está en processingFiles
          const isCurrentlyProcessing = processingFiles.has(file.object_name);

          return {
            ...file,
            status: isCurrentlyProcessing && file.status === 'pending'
              ? 'analyzing'
              : file.status
          };
        });

        setFiles(mappedFiles);
      }
    } catch (error) {
      console.error('Error loading files:', error);
      setToastMessage('Connection error while loading files');
      setToastColor('danger');
      setShowToast(true);
    } finally {
      if (showLoading) setLoading(false);
    }
  };

  const deleteFiles = async (object_name: string, hasModified: number) => {
    try {
      setLoading(true);
      console.log(object_name, " - ", hasModified);
      const response = await backendServices.deleteFile(object_name, BUCKET_ORIGIN_NAME);
      console.log(response);
      if (response.success) {
        if (hasModified != 0) {
          const response_modify = await backendServices.deleteFile(object_name, BUCKET_MODIFY_NAME);
          if (response_modify) {
          }
        }
      }
    } catch (error) {
      console.error('Error deleting files:', error);
      setToastMessage('Error de conexión al eliminar archivos');
      setToastColor('danger');
      setShowToast(true);
    } finally {
      setLoading(false);
      await loadFiles();
    }
  };


  const downloadFile = async (objectName: string, bucketName: string) => {
    try {
      setDownloadingFile(objectName);
      const response = await backendServices.downloadFile(objectName, bucketName);

      if (response.success) {
        // El backend debería devolver un blob o URL para descargar
        // Por ahora mostramos un mensaje de éxito
        setToastMessage(`Descarga iniciada: ${objectName}`);
        setToastColor('success');
      } else {
        setToastMessage(response.error || 'Error al descargar archivo');
        setToastColor('danger');
      }
    } catch (error) {
      console.error('Error downloading file:', error);
      setToastMessage('Error de conexión al descargar archivo');
      setToastColor('danger');
    } finally {
      setDownloadingFile(null);
      setShowToast(true);
    }
  };

  const handleRefresh = async (event: CustomEvent<RefresherEventDetail>) => {
    await loadFiles();
    event.detail.complete();
  };

  const getFileName = (objectName: string): string => {
    // Extraer el nombre original del archivo si tiene el formato timestamp_id_filename
    const parts = objectName.split('_');
    if (parts.length >= 3) {
      return parts.slice(2).join('_');
    }
    return objectName;
  };

  const filteredFiles = files.filter(file =>
    getFileName(file.file_name).toLowerCase().includes(searchText.toLowerCase())
  );

  return (
    <IonPage>
      <IonContent style={{ '--background': '#d3d3d3ff' }}>
        <IonRefresher slot="fixed" onIonRefresh={handleRefresh}>
          <IonRefresherContent></IonRefresherContent>
        </IonRefresher>

        <div style={{
          maxWidth: '1200px',
          margin: '0 auto',
          padding: '24px'
        }}>
          {/* Page Title */}
          <div style={{
            fontSize: '12px',
            color: '#6c757d',
            marginBottom: '8px',
            textTransform: 'uppercase',
            letterSpacing: '0.5px'
          }}>
          </div>

          {/* Main Content Card */}
          <IonCard style={{
            borderRadius: '12px',
            boxShadow: '0 2px 10px rgba(0,0,0,0.08)',
            border: 'none',
            marginBottom: '0'
          }}>
            <IonCardContent style={{ padding: '32px' }}>
              {/* Header Section */}
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                marginBottom: '24px'
              }}>
                <div>
                  <h1 style={{
                    fontSize: '28px',
                    fontWeight: '700',
                    color: '#333',
                    marginBottom: '8px'
                  }}>
                    Analyzed Files
                  </h1>
                  <p style={{
                    color: '#6c757d',
                    fontSize: '14px',
                    margin: '0'
                  }}>
                    Manage and analyze your uploaded PDF documents. ({files.length} files)
                  </p>
                </div>
                <div style={{ display: 'flex', gap: '8px' }}>
                  <IonButton
                    fill="outline"
                    onClick={() => loadFiles()}
                    disabled={loading}
                    style={{
                      '--border-color': '#007bff',
                      '--color': '#007bff',
                      borderRadius: '8px',
                      fontSize: '14px',
                      fontWeight: '600',
                      height: '40px'
                    }}
                  >
                    <IonIcon icon={refreshOutline} style={{ marginRight: '6px' }} />
                    Refresh
                  </IonButton>
                  <IonButton
                    style={{
                      '--background': '#007bff',
                      '--color': 'white',
                      borderRadius: '8px',
                      fontSize: '14px',
                      fontWeight: '600',
                      height: '40px'
                    }}
                    onClick={() => {
                      history.replace("/home");
                      history.push("/home")
                      window.location.reload()
                    }}
                  >
                    <IonIcon icon={addOutline} style={{ marginRight: '6px' }} />
                    Upload File
                  </IonButton>
                </div>
              </div>

              <div style={{ display: 'flex', paddingBottom: 5 }}>
                <IonButton
                  style={{
                    '--background': '#007bff',
                    '--color': 'white',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    height: '40px'
                  }}
                  onClick={() => analyzeAll()}
                >
                  <IonIcon icon={layersOutline} style={{ marginRight: '6px' }} />
                  Analyze All
                </IonButton>

                <IonButton
                  style={{
                    '--background': '#007bff',
                    '--color': 'white',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontWeight: '600',
                    height: '40px'
                  }}
                  expand="block" onClick={async () => {
                    await getPrompt()
                    setShowPromptModal(true);
                  }}
                >
                  <IonIcon icon={constructOutline} style={{ marginRight: '6px' }} />
                  AI Instructions
                </IonButton>

              </div>

              {/* Prompt Modal */}
              <IonModal style={{ '--height': '350px' } as React.CSSProperties} isOpen={showPromptModal} onDidDismiss={() => setShowPromptModal(false)}>
                <IonHeader>
                  <IonToolbar>
                    <IonTitle>AI Instrucciones</IonTitle>
                    <IonButton slot="end" fill="clear" onClick={() => setShowPromptModal(false)}>
                      Cerrar
                    </IonButton>

                  </IonToolbar>
                </IonHeader>

                <IonContent className="ion-padding">
                  <IonTextarea
                    placeholder="Escribe tus instrucciones aquí..."
                    value={prompt}
                    onIonChange={(e) => setPrompt(e.detail.value!)}
                    rows={6}
                  />

                </IonContent>
                <IonFooter className="ion-padding">
                  <IonButton disabled={loadingPrompt == false ? false : true} onClick={async () => await sendPrompt()}>
                    Enviar
                  </IonButton>
                  <IonButton disabled={loadingPrompt == false ? false : true} color="tertiary" onClick={async () => improvePrompt()}>
                    <IonIcon style={{ paddingRight: 5 }} size='small' icon={colorWandOutline} />
                    Mejorar
                  </IonButton>
                  <Tooltip content='Presiona "Mejorar" y después "Enviar" para usar la instrucción mejorada'>
                    <IonIcon style={{ paddingRight: 5 }} size='small' icon={alertCircleOutline} />
                  </Tooltip>
                </IonFooter>

              </IonModal>


              {/* Search Bar */}
              <IonSearchbar
                value={searchText}
                onIonInput={e => setSearchText(e.detail.value!)}
                placeholder="Search files by name..."
                disabled={loading}
                style={{
                  '--background': '#f8f9fa',
                  '--border-radius': '8px',
                  '--box-shadow': 'none',
                  '--color': '#333',
                  marginBottom: '24px',
                  padding: '0'
                }}
              />

              {/* Loading State */}
              {loading && (
                <div style={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  padding: '48px 0',
                  flexDirection: 'column',
                  gap: '16px'
                }}>
                  <IonSpinner name="crescent" style={{ width: '32px', height: '32px' }} />
                  <p style={{ color: '#6c757d', margin: 0 }}>Loading files...</p>
                </div>
              )}

              {/* Files List - Usando el nuevo componente */}
              {!loading && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                  {/* // Actualizar FileListItem en el render */}
                  {filteredFiles.map((file) => (
                    <FileListItem
                      key={file.object_name}
                      file={file}
                      downloadingFile={downloadingFile}
                      bucketName={BUCKET_ORIGIN_NAME}
                      bucketModifyName={BUCKET_MODIFY_NAME}
                      onAnalyzeFile={analyzeFile}
                      onDownloadFile={downloadFile}
                      onDeleteFile={deleteFiles}
                      isProcessing={processingFiles.has(file.object_name)}
                    />
                  ))}

                </div>
              )}

              {/* No Results */}
              {!loading && filteredFiles.length === 0 && files.length > 0 && (
                <div style={{
                  textAlign: 'center',
                  padding: '48px 0',
                  color: '#6c757d'
                }}>
                  <p>No files found matching your search.</p>
                </div>
              )}

              {/* No Files */}
              {!loading && files.length === 0 && (
                <div style={{
                  textAlign: 'center',
                  padding: '48px 0',
                  color: '#6c757d'
                }}>
                  <p>No files uploaded yet. Click "Upload File" to get started.</p>
                </div>
              )}
            </IonCardContent>
          </IonCard>
        </div>

        <IonToast
          isOpen={showToast}
          onDidDismiss={() => setShowToast(false)}
          message={toastMessage}
          duration={3000}
          color={toastColor}
          position="top"
        />
      </IonContent>
    </IonPage>
  );
};

export default PDFAnalyzerFiles;
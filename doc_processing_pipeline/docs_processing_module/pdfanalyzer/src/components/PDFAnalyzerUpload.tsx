import React, { useState } from 'react';
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
  IonSpinner,
  IonToast,
  useIonRouter
} from '@ionic/react';
import { documentOutline, cloudUploadOutline, trashOutline } from 'ionicons/icons';
import backendServices from '../services/backendServices';

const PDFAnalyzerUpload: React.FC = () => {
  const [dragActive, setDragActive] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [showToast, setShowToast] = useState(false);
  const [toastMessage, setToastMessage] = useState('');
  const [toastColor, setToastColor] = useState<'success' | 'danger'>('success');
  const ionRouter = useIonRouter();

  const uploadFiles = async () => {
    if (selectedFiles.length === 0) {
      setToastMessage('No hay archivos seleccionados para enviar');
      setToastColor('danger');
      setShowToast(true);
      return;
    }

    try {
      setLoading(true);
      const response = await backendServices.uploadFiles(selectedFiles);

      if (response.success) {
        setToastMessage(`${selectedFiles.length} archivo(s) enviado(s) con éxito`);
        setToastColor('success');
        setSelectedFiles([]); // Limpiar la lista después del envío exitoso
        ionRouter.push(`/uploaded-files`, "forward");
      } else {
        setToastMessage(response.error || 'Error al enviar los archivos');
        setToastColor('danger');
      }
    } catch (err) {
      console.error('Error uploading files:', err);
      setToastMessage('Error de conexión al enviar los archivos');
      setToastColor('danger');
    } finally {
      setLoading(false);
      setShowToast(true);
    }
  };

  const removeFile = (indexToRemove: number) => {
    setSelectedFiles(prev => prev.filter((_, index) => index !== indexToRemove));
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const files = Array.from(e.dataTransfer.files).filter(
        file => file.type === 'application/pdf'
      );
      setSelectedFiles(prev => [...prev, ...files]);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files).filter(
        file => file.type === 'application/pdf'
      );
      setSelectedFiles(prev => [...prev, ...files]);
    }
  };

  const triggerFileSelect = () => {
    const fileInput = document.getElementById('file-input') as HTMLInputElement;
    fileInput?.click();
  };

  return (
    <IonPage>
      <IonHeader>
      </IonHeader>

      <IonContent className="ion-padding" style={{ '--background': '#f0f4f8' }}>
        <div style={{ 
          maxWidth: '800px', 
          margin: '0 auto',
          paddingTop: '60px'
        }}>
          {/* Main Card */}
          <IonCard style={{ 
            borderRadius: '16px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
            border: 'none'
          }}>
            <IonCardContent style={{ padding: '48px 32px' }}>
              {/* Title */}
              <h1 style={{ 
                textAlign: 'center', 
                fontSize: '32px', 
                fontWeight: 'bold',
                color: '#333',
                marginBottom: '16px'
              }}>
                Upload Your PDFs for Analysis
              </h1>

              {/* Subtitle */}
              <p style={{ 
                textAlign: 'center',
                color: '#6c757d',
                fontSize: '16px',
                lineHeight: '1.5',
                marginBottom: '48px',
                maxWidth: '500px',
                margin: '0 auto 48px auto'
              }}>
                Drag and drop files, or click to select them. We analyze file size, metadata, and more. Multiple file uploads are supported.
              </p>

              {/* Drag & Drop Area */}
              <div
                onDragEnter={handleDrag}
                onDragLeave={handleDrag}
                onDragOver={handleDrag}
                onDrop={handleDrop}
                style={{
                  border: `2px dashed ${dragActive ? '#3554dcff' : '#dee2e6'}`,
                  borderRadius: '12px',
                  padding: '64px 32px',
                  textAlign: 'center',
                  backgroundColor: dragActive ? '#fff5f5' : '#f8f9fa',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer',
                  minHeight: '200px',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}
                onClick={triggerFileSelect}
              >
                <IonIcon
                  icon={documentOutline}
                  style={{
                    fontSize: '48px',
                    color: '#3554dcff',
                    marginBottom: '16px'
                  }}
                />
                
                <div style={{ 
                  fontSize: '18px', 
                  fontWeight: '600',
                  color: '#333',
                  marginBottom: '8px'
                }}>
                  Drag and drop files here
                </div>
                
                <div style={{ 
                  fontSize: '14px', 
                  color: '#6c757d',
                  marginBottom: '24px'
                }}>
                  or
                </div>

                <IonButton
                  style={{
                    '--background': '#3554dcff',
                    '--color': 'white',
                    borderRadius: '24px',
                    fontSize: '16px',
                    fontWeight: '600',
                    padding: '12px 32px'
                  }}
                >
                  Browse Files
                </IonButton>

                <input
                  id="file-input"
                  type="file"
                  multiple
                  accept=".pdf"
                  onChange={handleFileSelect}
                  style={{ display: 'none' }}
                />
              </div>

              {/* Selected Files List */}
              {selectedFiles.length > 0 && (
                <div style={{ marginTop: '32px' }}>
                  <div style={{ 
                    display: 'flex', 
                    justifyContent: 'space-between', 
                    alignItems: 'center',
                    marginBottom: '16px'
                  }}>
                    <h3 style={{ 
                      fontSize: '18px', 
                      fontWeight: '600',
                      color: '#333',
                      margin: 0
                    }}>
                      Selected Files ({selectedFiles.length})
                    </h3>
                    
                    <IonButton
                      onClick={uploadFiles}
                      disabled={loading || selectedFiles.length === 0}
                      style={{
                        '--background': '#28a745',
                        '--color': 'white',
                        borderRadius: '20px',
                        fontSize: '14px',
                        fontWeight: '600'
                      }}
                    >
                      {loading ? (
                        <>
                          <IonSpinner name="crescent" style={{ marginRight: '8px' }} />
                          Enviando...
                        </>
                      ) : (
                        <>
                          <IonIcon icon={cloudUploadOutline} style={{ marginRight: '8px' }} />
                          Enviar Archivos
                        </>
                      )}
                    </IonButton>
                  </div>
                  
                  <IonList>
                    {selectedFiles.map((file, index) => (
                      <IonItem key={index}>
                        <IonIcon 
                          icon={documentOutline} 
                          slot="start" 
                          style={{ color: '#dc3545' }}
                        />
                        <IonLabel>
                          <h3>{file.name}</h3>
                          <p>{(file.size / 1024 / 1024).toFixed(2)} MB</p>
                        </IonLabel>
                        <IonButton
                          fill="clear"
                          slot="end"
                          onClick={(e) => {
                            e.stopPropagation();
                            removeFile(index);
                          }}
                          style={{ '--color': '#dc3545' }}
                        >
                          <IonIcon icon={trashOutline} />
                        </IonButton>
                      </IonItem>
                    ))}
                  </IonList>
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

export default PDFAnalyzerUpload;
import React, { useState, useRef, useEffect } from 'react';
import {
  IonPage,
  IonContent,
  IonButton,
  IonIcon,
  IonModal,
  IonInput,
  IonSpinner,
  IonToast,
} from '@ionic/react';
import {
  checkmarkCircleOutline,
  closeCircleOutline,
  sendSharp,
  chevronDownOutline,
  documentTextOutline,
  closeOutline,
  documentOutline,
  trash,
} from 'ionicons/icons';
import invoiceValidatorService from '../../services/InvoiceValidatorService';
import './InvoiceValidator.css'

// Types
interface ValidationHistoryResponse {
  success: boolean;
  data: ValidationItem[];
  error?: string;
}

interface ValidationItem {
  validationId: string;
  invoiceNumber: string;
  files: Record<string, string[]>;
  fields: any[];
  details: any[];
  timestamp: string;
}

interface FieldComparison {
  campo: string;
  logistica: string;
  sat: string;
  estado: 'success' | 'danger';
}

interface DetailComparison extends FieldComparison { }

interface ValidationResultData {
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

interface ValidationResponse {
  success: boolean;
  data?: ValidationResultData;
  error?: string;
}

interface SendObservationsRequest {
  email: string;
  validationId: string;
  invoiceNumber: string;
  fields: FieldComparison[];
}

interface SendObservationsResponse {
  success: boolean;
  error?: string;
}

interface ComparacionFilaProps {
  campo: string;
  logistica: string;
  sat: string;
  estado: 'success' | 'danger';
  esDetalle?: boolean;
}

interface ToastState {
  show: boolean;
  message: string;
  color: 'success' | 'danger' | 'warning';
}

interface CurrentValidation {
  id: string;
  invoiceNumber: string;
  fields: FieldComparison[];
}

interface SavedValidation extends ValidationResultData {
  isSaved?: boolean;
}

// ===== COMPONENTES =====

const ComparacionFila: React.FC<ComparacionFilaProps> = ({ campo, logistica, sat, estado, esDetalle = false }) => {
  const isSuccess = estado === 'success';
  const estadoIcono = isSuccess ? checkmarkCircleOutline : closeCircleOutline;
  const iconoColorClass = isSuccess ? 'icon-success' : 'icon-danger';
  const bgClass = isSuccess ? 'bg-row-success' : 'bg-row-danger';

  return (
    <tr className={`data-row ${esDetalle ? 'row-detail' : ''}`}>
      <td className="cell-label">{campo}</td>
      <td className={`cell-data ${bgClass}`}>{logistica}</td>
      <td className={`cell-data ${bgClass}`}>{sat}</td>
      <td className="cell-status">
        <IonIcon icon={estadoIcono} className={`status-icon ${iconoColorClass}`} />
      </td>
    </tr>
  );
};

interface FileUploadAreaProps {
  title: string;
  description: string;
  files: File[];
  onFilesChange: (files: File[]) => void;
}

const FileUploadArea: React.FC<FileUploadAreaProps> = ({ title, description, files, onFilesChange }) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);

    const droppedFiles = Array.from(e.dataTransfer.files);
    onFilesChange([...files, ...droppedFiles]);
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const selectedFiles = Array.from(e.target.files);
      onFilesChange([...files, ...selectedFiles]);
    }
    e.target.value = '';
  };

  const handleRemoveFile = (index: number) => {
    const newFiles = files.filter((_, i) => i !== index);
    onFilesChange(newFiles);
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="upload-box">
      <div className="upload-content">
        <strong className="upload-box-title">{title}</strong>

        {files.length > 0 && (
          <div className="files-list">
            <p className="upload-box-desc" style={{ marginBottom: '8px' }}>Archivos cargados:</p>
            {files.map((file, index) => (
              <div key={index} className="file-item">
                <IonIcon icon={documentOutline} className="file-icon" />
                <span className="file-name">{file.name}</span>
                <button
                  className="remove-file-btn"
                  onClick={() => handleRemoveFile(index)}
                >
                  <IonIcon icon={closeOutline} />
                </button>
              </div>
            ))}
          </div>
        )}

        <div
          className={`drop-zone ${isDragging ? 'dragging' : ''}`}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
        >
          <p className="upload-box-desc">{description}</p>
          <input
            ref={fileInputRef}
            type="file"
            multiple
            accept=".pdf,.xml,.xls,.xlsx"
            style={{ display: 'none' }}
            onChange={handleFileSelect}
          />
          <button
            className="custom-btn btn-secondary-blue"
            onClick={handleButtonClick}
          >
            {files.length > 0 ? 'Añadir más archivos' : 'Seleccionar Archivo'}
          </button>
        </div>
      </div>
    </div>
  );
};

interface ValidationResultCardProps {
  result: ValidationResultData;
  isLoading: boolean;
  isSaved?: boolean;
  onSendObservations: (validationId: string, invoiceNumber: string, fields: FieldComparison[]) => void;
  onDelete?: (validationId: string) => void;
}

const ValidationResultCard: React.FC<ValidationResultCardProps> = ({ result, isLoading, isSaved, onSendObservations, onDelete }) => {
  if (isLoading) {
    return (
      <div className="validation-result-card">
        <div className="loading-container">
          <IonSpinner name="crescent" />
          <p>Procesando validación...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="validation-result-card">
      <div className="card-header-flex">
        <div>
          <h3>Validación: Factura {result.invoiceNumber}</h3>
          <p className="result-files-info">
            Archivos: {result.files.logistic.join(', ')}, {result.files.sat.join(', ')}
          </p>
        </div>
        <div className="header-buttons-group">
          <button className="custom-btn btn-outline-gray">
            <IonIcon icon={documentTextOutline} />
            Generar Reporte PDF
          </button>
          <button
              className="custom-btn btn-primary-solid"
              onClick={() => onSendObservations(result.validationId, result.invoiceNumber, result.fields)}
            >
              <IonIcon icon={sendSharp} />
              Enviar Observaciones
            </button>
          {isSaved && onDelete && (
            <button
              className="custom-btn btn-outline-red"
              onClick={() => onDelete(result.validationId)}
            >
              <IonIcon icon={trash} />
              Eliminar
            </button>
          )}
        </div>
      </div>

      <div className="table-responsive">
        <table className="validator-table">
          <thead>
            <tr>
              <th style={{ width: '20%' }}>CAMPO</th>
              <th style={{ width: '35%' }}>DATOS FACTURA LOGÍSTICA</th>
              <th style={{ width: '35%' }}>DATOS DOCUMENTO SAT</th>
              <th style={{ width: '10%', textAlign: 'center' }}>ESTADO</th>
            </tr>
          </thead>
          <tbody>
            {result.fields.map((field, index) => (
              <ComparacionFila
                key={index}
                campo={field.campo}
                logistica={field.logistica}
                sat={field.sat}
                estado={field.estado}
              />
            ))}

            {result.details && result.details.length > 0 && (
              <tr>
                <td colSpan={4} className="detail-trigger-cell">
                  <details className="custom-details">
                    <summary>
                      Partidas ({result.details.length})
                      <IonIcon icon={chevronDownOutline} />
                    </summary>
                    <div className="details-content">
                      <table className="validator-table sub-table">
                        <thead>
                          <tr>
                            <th>Descripción</th>
                            <th>Cantidad (Logística)</th>
                            <th>Cantidad (SAT)</th>
                            <th className="text-center">Estado</th>
                          </tr>
                        </thead>
                        <tbody>
                          {result.details.map((detail, index) => (
                            <ComparacionFila
                              key={index}
                              campo={detail.campo}
                              logistica={detail.logistica}
                              sat={detail.sat}
                              estado={detail.estado}
                              esDetalle
                            />
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </details>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};


const InvoiceValidator: React.FC = () => {
  const [logisticFiles, setLogisticFiles] = useState<File[]>([]);
  const [satFiles, setSatFiles] = useState<File[]>([]);
  const [validationResults, setValidationResults] = useState<SavedValidation[]>([]);
  const [loadingStates, setLoadingStates] = useState<{ [key: string]: boolean }>({});
  const [showModal, setShowModal] = useState(false);
  const [email, setEmail] = useState('');
  const [currentValidation, setCurrentValidation] = useState<CurrentValidation | null>(null);
  const [isSending, setIsSending] = useState(false);
  const [isLoadingHistory, setIsLoadingHistory] = useState(false);
  const [toast, setToast] = useState<ToastState>({
    show: false,
    message: '',
    color: 'success'
  });

  const loadValidationHistory = async () => {
    setIsLoadingHistory(true);
    try {
      const response = await invoiceValidatorService.getValidationHistory();

      if (!response.success || !response.data) {
        console.error("Error cargando historial:", response.error);
        setIsLoadingHistory(false);
        return;
      }

      if (response.success) {
        const list: ValidationHistoryResponse = response.data.data
        console.log(response.data)

        if (Array.isArray(list)) {
          const historicalValidations = list.map(item => ({
            validationId: item.validationId,
            invoiceNumber: item.invoiceNumber,
            files: item.files,
            fields: item.fields,
            timestamp: item.timestamp,
            details: item.details,
            isSaved: true
          }));
          console.log(historicalValidations)

          setValidationResults(historicalValidations);
        } else {
          console.error("El historial no es un array:", list);
        }
      }

    } catch (error) {
      console.error('Error cargando historial:', error);
      setToast({
        show: true,
        message: 'Error al cargar el historial de validaciones',
        color: 'danger'
      });
    } finally {
      setIsLoadingHistory(false);
    }
  };

  const handleDeleteValidation = async (validationId: string) => {
    try {
      const response = await invoiceValidatorService.deleteValidation(validationId);

      if (response.success) {
        setValidationResults(prev => prev.filter(r => r.validationId !== validationId));
        setToast({
          show: true,
          message: 'Registro eliminado exitosamente',
          color: 'success'
        });
      } else {
        setToast({
          show: true,
          message: response.error || 'Error al eliminar el registro',
          color: 'danger'
        });
      }

    } catch (error) {
      console.error('Error eliminando validación:', error);
      setToast({
        show: true,
        message: 'Error de conexión al eliminar',
        color: 'danger'
      });
    }
  };

  const handleInitValidation = async () => {
    if (logisticFiles.length === 0 || satFiles.length === 0) {
      setToast({
        show: true,
        message: 'Por favor, cargue archivos en ambas secciones',
        color: 'warning'
      });
      return;
    }

    // Mostrar toast de procesamiento
    setToast({
      show: true,
      message: 'Su solicitud está siendo procesada...',
      color: 'warning'
    });

    // Crear un ID temporal para esta validación
    const tempId = `temp-${Date.now()}`;

    // Crear resultado temporal con estado de carga
    const tempResult: SavedValidation = {
      validationId: tempId,
      invoiceNumber: 'Procesando...',
      files: {
        logistic: logisticFiles.map((f: File) => f.name),
        sat: satFiles.map((f: File) => f.name)
      },
      fields: [],
      timestamp: new Date().toISOString(),
      isSaved: false
    };

    // Agregar a resultados y marcar como cargando
    setValidationResults(prev => [tempResult, ...prev]);
    setLoadingStates(prev => ({ ...prev, [tempId]: true }));

    try {
      const response = await invoiceValidatorService.validateInvoices(logisticFiles, satFiles);

      if (response.success && response.data) {
        // Reemplazar el resultado temporal con el real
        setValidationResults(prev =>
          prev.map(r => r.validationId === tempId ? { ...response.data!, isSaved: false } : r)
        );

        setToast({
          show: true,
          message: 'Validación completada exitosamente',
          color: 'success'
        });
      } else {
        // Remover el resultado temporal si hay error
        setValidationResults(prev => prev.filter(r => r.validationId !== tempId));

        setToast({
          show: true,
          message: response.error || 'Error al validar las facturas',
          color: 'danger'
        });
      }
    } catch (error) {
      setValidationResults(prev => prev.filter(r => r.validationId !== tempId));

      setToast({
        show: true,
        message: 'Error de conexión con el servidor',
        color: 'danger'
      });
    } finally {
      // Limpiar estado de carga
      setLoadingStates(prev => {
        const newState = { ...prev };
        delete newState[tempId];
        return newState;
      });

      // Limpiar archivos para nueva carga
      setLogisticFiles([]);
      setSatFiles([]);
    }
  };

  const handleOpenModal = (validationId: string, invoiceNumber: string, fields: FieldComparison[]) => {
    setCurrentValidation({ id: validationId, invoiceNumber, fields });
    setShowModal(true);
  };

  const handleSendObservations = async () => {
    if (!email || !currentValidation) {
      setToast({
        show: true,
        message: 'Por favor, ingrese un correo electrónico válido',
        color: 'warning'
      });
      return;
    }

    setIsSending(true);

    try {
      const response = await invoiceValidatorService.sendObservations({
        email,
        validationId: currentValidation.id,
        invoiceNumber: currentValidation.invoiceNumber,
        fields: currentValidation.fields
      });

      if (response.success) {
        setToast({
          show: true,
          message: 'Observaciones enviadas exitosamente',
          color: 'success'
        });
        setShowModal(false);
        setEmail('');
        setCurrentValidation(null);
      } else {
        setToast({
          show: true,
          message: response.error || 'Error al enviar las observaciones',
          color: 'danger'
        });
      }
    } catch (error) {
      setToast({
        show: true,
        message: 'Error de conexión con el servidor',
        color: 'danger'
      });
    } finally {
      setIsSending(false);
    }
  };

  return (
    <IonPage>
      <IonContent>
        <div className="app-background">
          <div className="main-wrapper">

            <header className="page-header">
              <h1 className="app-title">Validación de Facturas</h1>
              <p className="app-subtitle">
                Cargue los documentos de la factura logística y del SAT para comparar y validar los datos.
              </p>
            </header>

            {/* Card 1: Carga de Documentos */}
            <section className="card-container">
              <div className="card-header-line">
                <h2>1. Cargar Documentos</h2>
              </div>

              <div className="upload-grid-container">
                <FileUploadArea
                  title="Factura Logística (PDF & XLSX)"
                  description="Arrastra y suelta el archivo PDF o haz clic para seleccionar."
                  files={logisticFiles}
                  onFilesChange={setLogisticFiles}
                />

                <FileUploadArea
                  title="Documento SAT (PDF o XML)"
                  description="Arrastra y suelta el archivo PDF o XML o haz clic para seleccionar."
                  files={satFiles}
                  onFilesChange={setSatFiles}
                />
              </div>

              <div className="card-actions-right">
                <button
                  className="custom-btn btn-primary-soft"
                  onClick={handleInitValidation}
                  disabled={logisticFiles.length === 0 || satFiles.length === 0}
                >
                  Iniciar Validación
                </button>
              </div>
            </section>

            {/* Card 2: Resultados - Lista dinámica */}
            {validationResults.length > 0 && (
              <section className="card-container mt-large">
                <div className="card-header-line">
                  <h2>2. Resultados de la Comparación</h2>
                </div>

                {validationResults.map((result) => (
                  <ValidationResultCard
                    key={result.validationId}
                    result={result}
                    isLoading={loadingStates[result.validationId] || false}
                    isSaved={result.isSaved}
                    onSendObservations={handleOpenModal}
                    onDelete={handleDeleteValidation}
                  />
                ))}
              </section>
            )}

            {/* Card 3: Historial de Validaciones */}
            {validationResults.length === 0 && (
              <section className="card-container mt-large">
                <div className="card-header-line">
                  <h2>2. Historial de Validaciones</h2>
                </div>

                <div className="card-actions-right">
                  <button
                    className="custom-btn btn-primary-soft"
                    onClick={loadValidationHistory}
                    disabled={isLoadingHistory}
                  >
                    {isLoadingHistory ? 'Cargando...' : 'Cargar Historial'}
                  </button>
                </div>
              </section>
            )}

          </div>
        </div>

        {/* Modal de Enviar Observaciones */}
        <IonModal isOpen={showModal} onDidDismiss={() => setShowModal(false)}>
          <div className="modal-container">
            <div className="modal-header">
              <h2>Enviar Observaciones de la Factura {currentValidation?.invoiceNumber}</h2>
              <button className="modal-close-btn" onClick={() => setShowModal(false)}>
                <IonIcon icon={closeOutline} />
              </button>
            </div>
            <p className="modal-subtitle">
              Se enviará un resumen de estas validaciones al correo especificado.
            </p>

            <div className="modal-body">
              <label className="input-label">Correo del Destinatario</label>
              <IonInput
                type="email"
                placeholder="ejemplo@proveedor.com"
                value={email}
                onIonInput={(e) => setEmail(e.detail.value || '')}
                className="custom-input"
              />

              <div className="validation-summary">
                <h3>Resumen de la Validación</h3>
                <table className="summary-table">
                  <thead>
                    <tr>
                      <th>CAMPO</th>
                      <th>DATOS EXTRAÍDOS (OCR)</th>
                      <th>DATOS DEL SISTEMA (REFERENCIA)</th>
                    </tr>
                  </thead>
                  <tbody>
                    {currentValidation?.fields.map((field, index) => (
                      <tr key={index} className={field.estado === 'success' ? 'row-success' : 'row-danger'}>
                        <td>{field.campo}</td>
                        <td>{field.logistica}</td>
                        <td>{field.sat}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="modal-footer">
              <button
                className="custom-btn btn-outline-gray"
                onClick={() => setShowModal(false)}
              >
                Cancelar
              </button>
              <button
                className="custom-btn btn-primary-solid"
                onClick={handleSendObservations}
                disabled={isSending || !email}
              >
                {isSending ? (
                  <>
                    <IonSpinner name="crescent" style={{ width: '16px', height: '16px', marginRight: '8px' }} />
                    Enviando...
                  </>
                ) : (
                  'Enviar Observaciones'
                )}
              </button>
            </div>
          </div>
        </IonModal>

        <IonToast
          isOpen={toast.show}
          message={toast.message}
          duration={3000}
          color={toast.color}
          onDidDismiss={() => setToast({ ...toast, show: false })}
        />
      </IonContent>
    </IonPage>
  );
};

export default InvoiceValidator;
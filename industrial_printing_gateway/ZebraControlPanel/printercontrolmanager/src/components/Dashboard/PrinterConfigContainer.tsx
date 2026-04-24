import React, { useState } from 'react';
import {
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonFooter,
    IonButton,
    IonInput,
    IonSelect,
    IonSelectOption,
    IonToggle,
    IonIcon,
    IonGrid,
    IonRow,
    IonCol,
    IonItem,
    IonLabel,
    IonCard,
    IonCardContent,
    IonText,
    IonSpinner,
    useIonAlert,
    IonCardHeader,
} from '@ionic/react';
import {
    settingsOutline,
    printOutline,
    saveOutline,
    closeOutline,
    optionsOutline,
    informationCircleOutline,
    rocketOutline,
    warningOutline,
    copyOutline,
} from 'ionicons/icons';
import './PrinterConfigContainer.css';

import { PrinterStoraged } from './PrinterContainer';
import { printerService } from '../../services/printerServices';

interface PrinterConfigModalProps {
    isOpen: boolean;
    onClose: () => void;
    printerData: PrinterStoraged;
    onRefresh: () => void;
}

export const PrinterConfigModal: React.FC<PrinterConfigModalProps> = ({
    isOpen,
    onClose,
    printerData,
    onRefresh
}) => {
    const [present] = useIonAlert();
    const [loading, setLoading] = useState(false);

    // Form States
    const [alias, setAlias] = useState(printerData.alias);
    const [driverType, setDriverType] = useState('ZPL II (Zebra)');
    const [paperWidth, setPaperWidth] = useState('104');
    const [speed, setSpeed] = useState('4 ips');
    const [debugMode, setDebugMode] = useState(printerData.debug_mode);
    const [autocut, setAutocut] = useState(true);

    const handleSaveChanges = async () => {
        setLoading(true);
        try {

            try {
                const response = await printerService.updatePrinterConfig({
                    id: printerData.id,
                    status: printerData.status,
                    port: printerData.port,
                    vendor_id: printerData.vendor_id,
                    product_id: printerData.product_id,
                    alias: alias,
                    debug_mode: printerData.debug_mode,
                });
                if (response.success) {
                    await present({
                        header: 'Éxito',
                        message: 'Cambios guardados correctamente',
                        buttons: ['OK'],
                    });
                    onRefresh();
                }
            } catch (error) {
                console.log(error);
            }


        } catch (error) {
            await present({
                header: 'Error',
                message: 'No se pudieron guardar los cambios',
                buttons: ['OK'],
            });
        } finally {
            setLoading(false);
        }
    };

    // const handleTestPrint = () => {
    //     console.log('Enviando impresión de prueba...');
    //     present({
    //         header: 'Impresión de Prueba',
    //         message: 'Enviando documento de prueba a la impresora...',
    //         buttons: ['OK'],
    //     });
    // };

    const handleUnlink = async () => {
        present({
            header: 'Desvinvular Impresora',
            message: '¿Estás seguro? Se eliminarán todos los ajustes personalizados.',
            buttons: [
                {
                    text: 'Cancelar',
                    role: 'cancel',
                },
                {
                    text: 'Desvincular',
                    role: 'destructive',
                    handler: async () => {
                        const response = await printerService.deletePrinterById(printerData.id);
                        if (response.success) {
                            onRefresh();
                            onClose()
                        }
                        console.log('Impresora desvinculada');
                    },
                },
            ],
        });
    };

    const copyToClipboard = async (text: string) => {
        try {
            await navigator.clipboard.writeText(text);
            present({
                header: 'Copiado',
                message: 'Endpoint copiado al portapapeles',
                buttons: ['OK'],
            });
        } catch (err) {
            console.error('Error al copiar:', err);
        }
    };

    return (
        <IonModal isOpen={isOpen} onDidDismiss={onClose} className="printer-config-modal">
            {/* Header */}
            <IonHeader>
                <IonToolbar className="printer-config-header">
                    <div className="printer-config-title">
                        <div className="printer-config-icon">
                            <IonIcon icon={settingsOutline} />
                        </div>
                        <div className="printer-config-header-text">
                            <IonTitle>Configuración de Impresora</IonTitle>
                            <p className="printer-config-id">ID: {printerData.name}</p>
                        </div>
                    </div>
                    <div className="printer-config-actions" slot="end">
                        {/* <IonButton
                            fill="outline"
                            size="small"
                            onClick={handleTestPrint}
                            className="test-print-btn"
                        >
                            <IonIcon icon={printOutline} slot="start" />
                            Imprimir Prueba
                        </IonButton> */}

                        <IonButton
                            fill="clear"
                            onClick={onClose}
                            size="small"
                            className="close-btn"
                        >
                            <IonIcon icon={closeOutline} />
                        </IonButton>
                    </div>

                </IonToolbar>
            </IonHeader>

            {/* Content */}
            <IonContent className="printer-config-content">
                {/* Parámetros Generales */}
                <div className="config-section">
                    <div className="section-header">
                        <IonIcon icon={optionsOutline} />
                        <h3>Parámetros Generales</h3>
                        <div>
                            <IonButton
                                color="primary"
                                size="small"
                                onClick={handleSaveChanges}
                                disabled={loading}
                            >
                                <IonIcon icon={saveOutline} slot="start" />
                                {loading ? 'Guardando...' : 'Guardar Cambios'}
                            </IonButton>
                        </div>
                    </div>
                    <IonGrid className="form-grid">
                        <IonRow>
                            <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                                <IonItem>
                                    <IonLabel position="stacked">Alias de Impresora</IonLabel>
                                    <IonInput
                                        type="text"
                                        value={alias}
                                        onIonChange={(e) => setAlias(e.detail.value || '')}
                                        placeholder="Nombre de la impresora"
                                    />
                                </IonItem>
                            </IonCol>
                            {/* 
              <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                <IonItem>
                  <IonLabel position="stacked">Tipo de Driver</IonLabel>
                  <IonSelect
                    value={driverType}
                    onIonChange={(e) => setDriverType(e.detail.value)}
                  >
                    <IonSelectOption>ZPL II (Zebra)</IonSelectOption>
                    <IonSelectOption>EPL (Eltron)</IonSelectOption>
                    <IonSelectOption>Generic / Text Only</IonSelectOption>
                  </IonSelect>
                </IonItem>
              </IonCol>

              <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                <IonItem>
                  <IonLabel position="stacked">Ancho de Papel</IonLabel>
                  <IonInput
                    type="number"
                    value={paperWidth}
                    onIonChange={(e) => setPaperWidth(e.detail.value || '')}
                    placeholder="104"
                  />
                  <span className="paper-unit">MM</span>
                </IonItem>
              </IonCol> */}
                            {/* 
              <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                <IonItem>
                  <IonLabel position="stacked">Velocidad</IonLabel>
                  <IonSelect
                    value={speed}
                    onIonChange={(e) => setSpeed(e.detail.value)}
                  >
                    <IonSelectOption>2 ips</IonSelectOption>
                    <IonSelectOption>4 ips</IonSelectOption>
                    <IonSelectOption>6 ips</IonSelectOption>
                  </IonSelect>
                </IonItem>
              </IonCol> */}

                            <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                                <IonCard className="toggle-card">
                                    <div className="toggle-content">
                                        <div>
                                            <h5>Modo Debug</h5>
                                            <p>Logs raw</p>
                                        </div>
                                        <IonToggle
                                            disabled
                                            checked={debugMode}
                                            onIonChange={(e) => setDebugMode(e.detail.checked)}
                                        />
                                    </div>
                                </IonCard>
                            </IonCol>
                            {/* 
              <IonCol sizeXs="12" sizeSm="12" sizeMd="6" sizeLg="4">
                <IonCard className="toggle-card">
                  <div className="toggle-content">
                    <div>
                      <h5>Corte Automático</h5>
                      <p>Post-etiqueta</p>
                    </div>
                    <IonToggle
                      checked={autocut}
                      onIonChange={(e) => setAutocut(e.detail.checked)}
                    />
                  </div>
                </IonCard>
              </IonCol> */}
                        </IonRow>
                    </IonGrid>
                </div>

                {/* Información Técnica y API Endpoint */}
                <div className="tech-section">
                    <IonGrid>
                        <IonRow>
                            {/* Información Técnica */}
                            <IonCol sizeXs="12" sizeMd="6">
                                <div className="config-section">
                                    <div className="section-header">
                                        <IonIcon icon={informationCircleOutline} />
                                        <h3>Información Técnica</h3>
                                    </div>

                                    <IonCard className="tech-card">
                                        <div className="tech-item">
                                            <span>Puerto de Conexión</span>
                                            <span>{printerData.port}</span>
                                        </div>
                                        <div className="tech-item">
                                            <span>Vendor ID (VID)</span>
                                            <span>{'0x'}{printerData.vendor_id}</span>
                                        </div>
                                        <div className="tech-item">
                                            <span>Product ID (PID)</span>
                                            <span>{'0x'}{printerData.product_id}</span>
                                        </div>
                                    </IonCard>
                                </div>
                            </IonCol>

                            {/* API Endpoint */}
                            <IonCol sizeXs="12" sizeMd="6">
                                <div className="config-section">
                                    <div className="section-header">
                                        <IonIcon icon={rocketOutline} className="api-icon" />
                                        <h3>API Endpoint</h3>
                                    </div>

                                    <IonCard className="api-card">
                                        <IonCardHeader style={{ padding: '5px' }}>
                                            <p className="api-description">
                                                Peticiones POST con raw data (ZPL/SZPL) para impresión directa desde aplicaciones externas.
                                            </p>
                                        </IonCardHeader>
                                        <div className="api-endpoint">
                                            <code>http://0.0.0.0:8090/print-zpl</code>
                                            <IonButton
                                                fill="clear"
                                                size="small"
                                                onClick={() =>
                                                    copyToClipboard(
                                                        'http://0.0.0.0:8090/print-zpl'
                                                    )
                                                }
                                            >
                                                <IonIcon icon={copyOutline} />
                                            </IonButton>
                                        </div>
                                        <div className="api-endpoint prettyprint">
                                            <code>
                                                {JSON.stringify(
                                                    {
                                                        "printer_name": printerData.name,
                                                        "zpl_code": [
                                                            "string"]
                                                    }
                                                )}
                                            </code>
                                            <IonButton
                                                fill="clear"
                                                size="small"
                                                onClick={() => {
                                                    var json = JSON.stringify(
                                                        {
                                                            "printer_name": printerData.name,
                                                            "zpl_code": [
                                                                "string"]
                                                        }
                                                    )
                                                    copyToClipboard(
                                                        json
                                                    )
                                                }}
                                            >
                                                <IonIcon icon={copyOutline} />
                                            </IonButton>
                                        </div>
                                    </IonCard>
                                </div>
                            </IonCol>
                        </IonRow>
                    </IonGrid>
                </div>

                {/* Zona de Peligro */}
                <IonCard className="danger-zone">
                    {/* <div className="danger-icon">
                        <IonIcon icon={warningOutline} />
                    </div> */}
                    <div className="danger-content">
                        <h4>Zona de Peligro</h4>
                        <p>
                            Al desvincular la impresora se eliminarán todos los ajustes
                            personalizados y el acceso API. Esta acción no se puede deshacer.
                        </p>
                        <IonButton
                            color="danger"
                            fill="outline"
                            size="small"
                            onClick={handleUnlink}
                        >
                            Desvincular Impresora
                        </IonButton>
                    </div>
                </IonCard>
            </IonContent>

            {/* Footer */}
            <IonFooter>
                <IonToolbar className="printer-config-footer">
                    <IonText className="service-status">
                        Estado del Servicio: <span className={printerData.status == true ? "status-active" : "status-inactive"}>{printerData.status == true ? "Activo" : "Inactivo"}</span>
                    </IonText>
                    <IonButton fill="clear" onClick={onClose} slot="end">
                        Cancelar
                    </IonButton>
                </IonToolbar>
            </IonFooter>
        </IonModal>
    );
};

export default PrinterConfigModal;
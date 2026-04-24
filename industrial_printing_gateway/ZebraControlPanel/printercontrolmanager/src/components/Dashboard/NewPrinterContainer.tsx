import React, { useState, useEffect } from 'react';
import {
  IonButton,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonIcon,
  IonText,
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonButtons,
  IonFooter,
  IonGrid,
  IonRow,
  IonCol,
  IonSpinner,
} from '@ionic/react';
import { add, close, print, printOutline, refresh } from 'ionicons/icons';
import './NetPrinterContainer.css';
import { printerService } from '../../services/printerServices';

interface Printer {
  id: string;
  name: string;
  status: boolean;
  port: string;
  vendor_id: string | null
  product_id: string | null
  alias: string | null
  debug_mode: boolean
}

interface NewPrinterProps {
  onRefresh: () => void;
}


const AddPrinterModal: React.FC<{
  isOpen: boolean;
  onClose: () => void;
  onSelectPrinter: (printer: Printer) => void;
  onRefresh: () => void;
}> = ({ isOpen, onClose, onSelectPrinter, onRefresh }) => {
  const [printers, setPrinters] = useState<Printer[]>([]);
  const [selectedPrinter, setSelectedPrinter] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleRefresh = async () => {
    setLoading(true);
    try {
      const response = await printerService.listPrinters();
      if (response.success && response.data) {
        console.log(response.data);
        // response.data es: { printers: Array(5) }
        // Necesitas acceder a response.data.printers
        const printersArray = Array.isArray(response.data)
          ? response.data
          : response.data.printers || [];

        const formattedPrinters: Printer[] = printersArray.map((printer: any, index: number) => ({
          id: `${index}-${printer.name}`, // Agregué id que falta
          name: printer.name,
          status: printer.status,
          port: printer.info && printer.info.pPortName ? printer.info.pPortName : 'Desconocido',
          vendor_id: printer.conn.vid ? printer.conn.vid : 'ND',
          product_id: printer.conn.pid ? printer.conn.pid : 'ND',
          alias: printer.name,
          debug_mode: false
        }));
        setPrinters(formattedPrinters);
      }
      // await new Promise(resolve => setTimeout(resolve, 1000));
    } catch (error) {
      console.error('Error al actualizar impresoras:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleConnect = async () => {
    if (!selectedPrinter) return;

    try {
      const printer = printers.find(p => p.id === selectedPrinter);
      if (!printer) return;

      console.log("Impresora obtenida: ", printer);

      const response = await printerService.savePrinter(printer);

      if (response.success) {
        console.log(response.data);
        console.log('Conectando a la impresora:', printer);
        onSelectPrinter(printer);
        setSelectedPrinter(null);
        onRefresh();
        onClose();
      } else {
        console.error("Error al guardar la impresora:", response);
        // Aquí podrías agregar una alerta visual al usuario si falló el guardado
      }

    } catch (error) {
      console.error('Error en handleConnect:', error);
    } finally {
      // Cualquier limpieza necesaria
    }
  };

  return (
    <IonModal isOpen={isOpen} onDidDismiss={onClose}>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Añadir Impresora USB</IonTitle>
          <IonButtons slot="end">
            <IonButton onClick={onClose}>
              <IonIcon icon={close} />
            </IonButton>
          </IonButtons>
        </IonToolbar>
      </IonHeader>

      {/* Header fijo con descripción y botón de actualizar */}
      <div style={{ padding: '16px', backgroundColor: 'var(--ion-background-color)' }}>
        <IonText>
          <p style={{ fontSize: '14px', color: 'var(--ion-color-medium)', margin: '0 0 16px 0' }}>
            Seleccione una impresora detectada para vincularla con la API.
          </p>
        </IonText>

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <IonText>
            <h3 style={{ margin: 0 }}>Dispositivos Detectados</h3>
          </IonText>
          <IonButton color={'tertiary'} fill="outline" size="small" onClick={handleRefresh} disabled={loading}>
            <IonIcon icon={refresh} slot="start" />
            Actualizar
          </IonButton>
        </div>
      </div>

      {/* Solo la lista scrolleable */}
      <IonContent scrollX={false}>
        <div style={{ padding: '16px' }}>
          <IonList lines="none">
            {loading ? (
              <div style={{ display: 'flex', justifyContent: 'center', padding: '20px' }}>
                <IonSpinner />
              </div>
            ) : (
              printers.map((printer) => (
                <IonItem
                  key={printer.id}
                  onClick={() => printer.status === false && setSelectedPrinter(printer.id)}
                  button
                  disabled={printer.status === true}
                  style={{
                    borderRadius: '8px',
                    marginBottom: '8px',
                    border: selectedPrinter === printer.id ? '2px solid var(--ion-color-primary)' : '1px solid var(--ion-color-step-200)',
                    backgroundColor: selectedPrinter === printer.id ? 'var(--ion-color-primary-tint)' : 'transparent',
                    opacity: printer.status === true ? 0.5 : 1,
                    cursor: printer.status === false ? 'not-allowed' : 'pointer',
                    paddingInlineStart: '16px',
                    paddingInlineEnd: '16px',
                  }}
                >
                  {/* Icono de Impresora */}
                  <IonIcon
                    slot="start"
                    icon={printer.status === false ? printOutline : print}
                    style={{
                      fontSize: '32px',
                      color: selectedPrinter === printer.id ? 'var(--ion-color-primary)' : 'var(--ion-color-medium)',
                      marginRight: '16px'
                    }}
                  />

                  {/* Información de la Impresora */}
                  <IonLabel>
                    <h2 style={{ fontWeight: '600', marginBottom: '4px' }}>
                      {printer.name}
                    </h2>
                    <p style={{ fontSize: '12px', color: 'var(--ion-color-medium)' }}>
                      Puerto: {printer.port}
                    </p>
                  </IonLabel>

                  {/* Badge de Estado */}
                  <div slot="end" style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <IonBadge
                      color={printer.status === false ? 'success' : 'medium'}
                    >
                      {printer.status === false ? '● Listo' : '● Registrado'}
                    </IonBadge>

                    {/* Indicador de Selección */}
                    <div
                      style={{
                        width: '24px',
                        height: '24px',
                        borderRadius: '50%',
                        border: '2px solid',
                        borderColor: selectedPrinter === printer.id ? 'var(--ion-color-primary)' : 'var(--ion-color-medium)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        transition: 'all 0.2s ease'
                      }}
                    >
                      {selectedPrinter === printer.id && (
                        <div
                          style={{
                            width: '12px',
                            height: '12px',
                            borderRadius: '50%',
                            backgroundColor: 'var(--ion-color-primary)'
                          }}
                        />
                      )}
                    </div>
                  </div>
                </IonItem>
              ))
            )}
          </IonList>
        </div>
      </IonContent>

      {/* Footer con Botones */}
      <IonFooter>
        <IonToolbar>
          <IonButtons slot="end">
            <IonButton color={'danger'} onClick={onClose}>
              Cancelar
            </IonButton>
            <IonButton
              color="primary"
              onClick={handleConnect}
              disabled={!selectedPrinter}
            >
              Registrar
            </IonButton>
          </IonButtons>
        </IonToolbar>
      </IonFooter>
    </IonModal>
  );
};

const NewPrinterContainer: React.FC<NewPrinterProps> = ({ onRefresh }) => {
  const [showModal, setShowModal] = useState(false);

  const handleSelectPrinter = (printer: Printer) => {
    console.log('Impresora seleccionada:', printer);
    // Aquí puedes agregar la lógica para conectar la impresora
    // Por ejemplo: guardar en estado, hacer una llamada API, etc.
  };

  return (
    <>
      <IonCard onClick={() => setShowModal(true)} button>
        <IonCardContent
          style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '10px',
            backgroundColor: '#034deb1a',
            borderRadius: '8px',
            padding: '20px'
          }}
        >
          <IonButton
            disabled
            style={{
              '--background': '#ffffff4b' as any
            }}
            shape='round'
          >
            <IonIcon slot='icon-only' icon={add} style={{ color: '#ffffff' }} />
          </IonButton>
          <IonText>Añadir impresora</IonText>
        </IonCardContent>
      </IonCard>

      {/* Modal para Añadir Impresora */}
      <AddPrinterModal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        onSelectPrinter={handleSelectPrinter}
        onRefresh={onRefresh}
      />
    </>
  );
};

export default NewPrinterContainer;
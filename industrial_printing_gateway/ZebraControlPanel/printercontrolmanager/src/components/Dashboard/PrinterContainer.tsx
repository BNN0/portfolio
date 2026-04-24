import {
  IonCard,
  IonCardContent,
  IonCol,
  IonGrid,
  IonIcon,
  IonRow,
  IonText,
  IonToggle,
  IonButton,
  IonChip,
  ToggleCustomEvent,
  useIonViewDidEnter,
  useIonViewDidLeave,
} from '@ionic/react';
import './PrinterContainer.css';
import { printOutline, receiptOutline, settings, ellipse } from 'ionicons/icons';
import { useEffect, useRef, useState } from 'react';
import { printerService } from '../../services/printerServices';
import PrinterConfigModal from './PrinterConfigContainer';

export interface PrinterStoraged {
  id: number;
  name: string;
  status: boolean;
  port: string;
  vendor_id: string;
  product_id: string;
  alias: string;
  debug_mode: boolean;
  created_at: Date;
  updated_at: Date;
}

interface PrinterProps {
  printer_data: PrinterStoraged;
  onRefresh: () => void;
}

const PrinterContainer: React.FC<PrinterProps> = ({
  printer_data,
  onRefresh,
}) => {
  const [status, setStatus] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isVisible, setIsVisible] = useState<boolean>(true);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const [isChecked, setIsChecked] = useState<boolean>(printer_data.status);
  const [isConfigModalOpen, setIsConfigModalOpen] = useState(false);

  useIonViewDidEnter(() => {
    setIsVisible(true);
    console.log(`PrinterContainer ${printer_data.id} - Visible en pantalla`);
  });

  useIonViewDidLeave(() => {
    setIsVisible(false);
    console.log(`PrinterContainer ${printer_data.id} - Fuera de pantalla`);
  });

  const getPrinterStatus = async () => {
    try {
      const response = await printerService.getPrinterStatus(printer_data.id);
      if (response.success && response.data) {
        setStatus(response.data.status === true);
      }
    } catch (error) {
      console.log(error);
    } finally {
      setIsLoading(false);
    }
  };

  const testConnection = async () => {
    try {
      setIsLoading(true);
      const zpl_code = ['^XA^FO50,50^A0N,50,50^FDHello World!^FS^XZ'];
      const response = await printerService.testPrinterConnection({
        printer_name: printer_data.name,
        zpl_code: zpl_code,
      });
      if (response.success && response.data) {
        console.log('Impresion realizada para impresora: ', printer_data);
      }
    } catch (error) {
      console.log(error);
    } finally {
      setIsLoading(false);
    }
  };

  const validateToggle = async (
    event: ToggleCustomEvent<{ checked: boolean }>
  ) => {
    try {
      const response = await printerService.updatePrinterConfig({
        id: printer_data.id,
        status: event.detail.checked,
        port: printer_data.port,
        vendor_id: printer_data.vendor_id,
        product_id: printer_data.product_id,
        alias: printer_data.alias,
        debug_mode: printer_data.debug_mode,
      });
      if (response.success) {
        setIsChecked(event.detail.checked);
        console.log('Toggle actualizado:', event.detail.checked);
      }
    } catch (error) {
      console.log(error);
    }
  };

  useEffect(() => {
    if (isVisible) {
      getPrinterStatus();

      intervalRef.current = setInterval(() => {
        getPrinterStatus();
      }, 10000);

      console.log(`Polling iniciado para ${printer_data.alias}`);
    } else {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
        console.log(`Polling detenido para ${printer_data.alias}`);
      }
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [isVisible, printer_data.id]);

  return (
    <>
      <IonCard className="printer-card">
        <IonCardContent className="printer-card-content">
          {/* Header: Nombre y Estado */}
          <div className="printer-header">
            <div className="printer-info">
              <IonIcon
                icon={printOutline}
                className="printer-icon"
              />
              <div className="printer-details">
                <IonText className="printer-name">
                  {printer_data.alias}
                </IonText>
                <IonText className="printer-specs">
                  {printer_data.port} • {printer_data.name}
                </IonText>
              </div>
            </div>

            <IonChip
              className={`status-chip ${status ? 'connected' : 'disconnected'}`}
              color={status ? 'success' : 'danger'}
            >
              <IonIcon icon={ellipse} />
              <span className="status-text">
                {status ? 'En Línea' : 'No Conectada'}
              </span>
            </IonChip>
          </div>

          {/* Divider */}
          <div className="printer-divider"></div>

          {/* Action Buttons */}
          <div className="printer-actions">
            <IonButton
              className="test-button"
              color="primary"
              expand="block"
              onClick={() => testConnection()}
              disabled={!isChecked || isLoading}
            >
              <IonIcon icon={receiptOutline} slot="start" />
              <IonText>{isLoading ? 'Enviando...' : 'Test'}</IonText>
            </IonButton>

            <IonButton
              className="settings-button"
              color="medium"
              onClick={() => setIsConfigModalOpen(true)}
            >
              <IonIcon icon={settings} />
            </IonButton>
          </div>

          {/* Toggle Enable/Disable */}
          <div className="printer-toggle">
            <IonToggle
              className="enable-toggle"
              checked={isChecked}
              onIonChange={(event) => validateToggle(event)}
              labelPlacement="start"
            >
              <IonText className="toggle-label">
                {isChecked ? 'Habilitada' : 'Deshabilitada'}
              </IonText>
            </IonToggle>
          </div>
        </IonCardContent>
      </IonCard>

      <PrinterConfigModal
        isOpen={isConfigModalOpen}
        onClose={() => setIsConfigModalOpen(false)}
        printerData={printer_data}
        onRefresh={onRefresh}
      />
    </>
  );
};

export default PrinterContainer;
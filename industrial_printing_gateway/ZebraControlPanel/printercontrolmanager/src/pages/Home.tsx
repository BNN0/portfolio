import { IonButton, IonChip, IonCol, IonContent, IonGrid, IonHeader, IonIcon, IonPage, IonRow, IonText, IonTitle, IonToolbar } from '@ionic/react';
import ExploreContainer from '../components/ExploreContainer';
import './Home.css';
import { colorFill, printOutline, printSharp, reload, settings } from 'ionicons/icons';
import ServerInfoContainer from '../components/Dashboard/ServerInfoContainer';
import NetworkInfoContainer from '../components/Dashboard/NetworkInfoContainer';
import NewPrinterContainer from '../components/Dashboard/NewPrinterContainer';
import PrinterContainer, { PrinterStoraged } from '../components/Dashboard/PrinterContainer';
import { useEffect, useState } from 'react';
import serverService from '../services/serverServices';
import { printerService, setPrinterServiceBaseUrl } from '../services/printerServices';

import ServerAddressModal from '../components/Modals/ServerAddressModal';

interface HomeProps {
  handleServerStatusChange(status: 'online' | 'offline'): void;
  serverStatus?: 'online' | 'offline';
  printerServiceReady: boolean;
  backendInfo: { hostname: string; ip_address: string; port: string } | null;
}

const Home: React.FC<HomeProps> = ({ handleServerStatusChange, serverStatus, printerServiceReady, backendInfo }) => {
  const [loading, isLoading] = useState<Boolean>(false);
  const [printersStoraged, setPrintersStoraged] = useState<PrinterStoraged[]>([]);
  const [listLength, setListLength] = useState<number>();
  const [showServerModal, setShowServerModal] = useState<boolean>(false);
  const [isServerConfigured, setIsServerConfigured] = useState<boolean>(false);

  const checkServerConfig = () => {
    const storedConfig = localStorage.getItem('printer_server_config');
    if (storedConfig) {
      const { ip, port } = JSON.parse(storedConfig);
      setIsServerConfigured(!!(ip && port));
    } else {
      setIsServerConfigured(false);
    }
  };

  useEffect(() => {
    checkServerConfig();
  }, []);

  const handlePrinterList = async () => {
    try {
      isLoading(true)

      const response = await printerService.getStoragedPrinters();

      if (response.success && response.data) {
        const formattedList: PrinterStoraged[] = Array.isArray(response.data.printers) ? response.data.printers : []
        setPrintersStoraged(formattedList)
        setListLength(formattedList.length)
      }
    }
    catch (error) {
      console.log(error);
    }
    finally {
      isLoading(false)
    }
  }


  useEffect(() => {
    if (serverStatus === 'online' && printerServiceReady) {
      handlePrinterList()
    }
  }, [serverStatus, printerServiceReady])

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', width: '100%' }}>
              <IonIcon icon={printOutline} size='large' style={{ marginRight: '10px', marginLeft: '10px' }} />
              <div>
                <IonTitle size='large' style={{ padding: '3px', marginBottom: '0', fontWeight: 'bold' }}>
                  Administrador de Impresoras
                </IonTitle>
                <IonText style={{ fontSize: '12px', color: '#747474', paddingLeft: '3px' }}>
                  v0.0 - Dev
                </IonText>
              </div>
              <IonButton className='set-server-address' fill='clear' onClick={() => setShowServerModal(true)}>
                <IonIcon icon={settings}></IonIcon>
              </IonButton>
            </div>
          </div>
        </IonToolbar>
      </IonHeader>
      <IonContent fullscreen>
        <IonHeader collapse="condense">
          <IonToolbar>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                <IonIcon icon={printOutline} size='large' style={{ marginRight: '10px', marginLeft: '10px' }} />
                <div>
                  <IonTitle size='large' style={{ padding: '3px', marginBottom: '0' }}>
                    Administrador de Impresoras
                  </IonTitle>
                  <IonText style={{ fontSize: '12px', color: '#747474', paddingLeft: '3px' }}>
                    v0.0 - Dev
                  </IonText>
                </div>
              </div>
            </div>
          </IonToolbar>
        </IonHeader>
        <IonGrid>
          <IonRow>
            <IonCol size='12' size-md='5' style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
              <ServerInfoContainer onServerStatusChange={handleServerStatusChange} isConfigured={isServerConfigured} />
            </IonCol>
            <IonCol size='12' size-md='7' style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
              <NetworkInfoContainer serverStatus={serverStatus} backendInfo={backendInfo} />
            </IonCol>
          </IonRow>
          <IonRow>
            <IonCol>
              <div style={{ top: 0, left: 0, display: 'flex', alignItems: 'center', padding: '5px' }}>
                <IonIcon icon={printSharp} size='large' style={{ marginRight: '10px', marginLeft: '10px', color: '#034deb' }} />
                <IonText style={{ color: '#FFFF', fontWeight: 'bold', fontSize: '12px' }}>
                  Impresoras Registradas
                </IonText>
                <IonChip style={{ fontSize: '10px', marginLeft: '10px' }} color={'tertiary'}>{listLength}</IonChip>
                <IonChip onClick={() => handlePrinterList()}><IonIcon icon={reload} size='small' style={{ marginLeft: '0', color: '#AAAAAA' }} />
                  <IonText style={{ color: '#AAAAAA', fontSize: '10px', marginLeft: '0' }}> Actualizar Lista</IonText></IonChip>
              </div>
            </IonCol>
          </IonRow>
          <IonRow>
            <IonCol size='12' style={{ display: 'flex', flexWrap: 'wrap' }}>
              {serverStatus === 'online' && printerServiceReady && loading === false && printersStoraged && (
                <>
                  {printersStoraged.map(printer => (
                    <IonCol key={printer.id} size='12' sizeSm='4'>
                      <PrinterContainer printer_data={printer} onRefresh={handlePrinterList} />
                    </IonCol>
                  ))}
                </>
              )}
              <IonCol size='12' sizeSm='4'>
                <NewPrinterContainer onRefresh={handlePrinterList} />
              </IonCol>
            </IonCol>
          </IonRow>
        </IonGrid>
      </IonContent>
      <ServerAddressModal
        isOpen={showServerModal}
        onClose={() => setShowServerModal(false)}
        onServerConfigured={() => {
          handleServerStatusChange('offline');
          checkServerConfig();
        }}
      />
    </IonPage>
  );
};

export default Home;

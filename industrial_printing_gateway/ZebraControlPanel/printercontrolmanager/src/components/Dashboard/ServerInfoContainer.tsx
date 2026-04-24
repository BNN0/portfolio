import { IonButton, IonCard, IonCardContent, IonChip, IonCol, IonGrid, IonIcon, IonRow, IonText } from '@ionic/react';
import './ServerInfoContainer.css';
import { caretForwardCircle, cloud, cloudDone, cloudOffline, ellipse, reload, server, stopCircle } from 'ionicons/icons';
import { useEffect, useState } from 'react';
import serverService from '../../services/serverServices';
import { setPrinterServiceBaseUrl } from '../../services/printerServices';

interface ServerInfoProps {
  onServerStatusChange: (status: 'online' | 'offline') => void;
  isConfigured: boolean;
}

const ServerInfoContainer: React.FC<ServerInfoProps> = ({ onServerStatusChange, isConfigured }) => {

  const [serverStatus, setServerStatus] = useState<string>('inactive');
  const [restarting, setRestarting] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  const getServerStatus = async () => {
    try {
      const response = await serverService.healthStatus();
      if (response.success) {
        console.log(response.data.status)
        if (response.data.status === "SERVICE_STOPPED") {
          setServerStatus('inactive');
          onServerStatusChange('offline');
        }
        else {
          setServerStatus('active');
          onServerStatusChange('online');
        }
      }
    }
    catch (error) {
      console.error('Error fetching server status:', error);
      setServerStatus('inactive');
      onServerStatusChange('offline');
    }
  }

  const startServer = async () => {
    try {
      const response = await serverService.startServer();
      if (response.success) {
        console.log(response.data)
        getServerStatus();
      }
    }
    catch (error) {
      console.error('Error starting server:', error);
      setServerStatus('inactive');
    }
  }

  const stopServer = async () => {
    try {
      const response = await serverService.stopServer();
      if (response.success) {
        console.log(response.data)
        getServerStatus();
      }
    }
    catch (error) {
      console.error('Error stopping server:', error);
      setServerStatus('inactive');
    }
  }

  const restartServer = async () => {
    try {
      setRestarting(true);
      const response = await serverService.restartServer();
      if (response.success) {
        console.log(response.data)
        getServerStatus();
      }
    }
    catch (error) {
      console.error('Error restarting server:', error);
      setServerStatus('inactive');
    }
    finally {
      setRestarting(false);
    }
  }

  useEffect(() => {
    getServerStatus();
  }, [serverStatus])

  return (
    <IonCard >
      <IonCardContent>
        <IonGrid>
          <IonRow>
            {serverStatus !== 'active' ?
              (
                <>
                  <IonCol size='9'>
                    <IonChip style={{ fontSize: '10px' }} color={'danger'}>
                      <IonIcon style={{ marginRight: 5 }} icon={ellipse}></IonIcon>
                      SERVICIO INACTIVO
                    </IonChip>
                    <IonText>
                      <h1 style={{ color: '#ffff', fontWeight: 'bold' }}>Sistema Inactivo</h1>
                      <p style={{}}>El servicio de administración de impresoras no se está ejecutando. Por favor, inicie el servicio para gestionar las impresoras conectadas al servidor.</p>
                    </IonText>
                  </IonCol>
                  <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <IonIcon icon={cloudOffline} style={{ color: '#4375b659', fontSize: '64px', width: '100px', height: '100px' }}></IonIcon>
                  </IonCol>
                </>)
              :
              (
                <>
                  <IonCol size='9'>
                    <IonChip style={{ fontSize: '10px' }} color={'success'}>
                      <IonIcon style={{ marginRight: 5 }} icon={ellipse}></IonIcon>
                      SERVICIO ACTIVO
                    </IonChip>
                    <IonText>
                      <h1 style={{ color: '#ffff', fontWeight: 'bold' }}>Sistema Operativo</h1>
                      <p style={{}}>El servicio de administración de impresoras está escuchando peticiones entrantes.</p>
                    </IonText>
                  </IonCol>
                  <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <IonIcon icon={cloudDone} style={{ color: '#528fdf', fontSize: '64px', width: '100px', height: '100px' }}></IonIcon>
                  </IonCol>
                </>
              )}
          </IonRow>
          <IonRow>
            <IonCol>
              {serverStatus !== 'active' ?
                (
                  <>
                    <IonButton disabled={!isConfigured || restarting} onClick={() => startServer()} expand="block" fill='outline' color='success'>
                      <IonIcon slot='start' icon={caretForwardCircle}></IonIcon>
                      <IonText>
                        <p style={{ fontWeight: 'bold', textTransform: 'none', color: '#ffff', margin: 0 }}>Iniciar</p>
                      </IonText>
                    </IonButton>
                  </>
                )
                :
                (
                  <>
                    <IonButton disabled={!isConfigured || restarting} onClick={() => stopServer()} expand="block" fill='outline' color='danger'>
                      <IonIcon slot='start' icon={stopCircle}></IonIcon>
                      <IonText>
                        <p style={{ fontWeight: 'bold', textTransform: 'none', color: '#ffff', margin: 0 }}>Detener</p>
                      </IonText>
                    </IonButton>
                  </>
                )
              }
            </IonCol>
            <IonCol>
              <IonButton disabled={!isConfigured || restarting} onClick={() => restartServer()} expand="block" fill='outline' color='medium'>
                <IonIcon slot='start' icon={reload}></IonIcon>
                <IonText>
                  <p style={{ fontWeight: 'bold', textTransform: 'none', color: '#ffff', margin: 0 }}>Reiniciar</p>
                </IonText>
              </IonButton>
            </IonCol>
          </IonRow>
        </IonGrid>
      </IonCardContent>
    </IonCard>
  );
};

export default ServerInfoContainer;

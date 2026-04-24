import { Redirect, Route } from 'react-router-dom';
import { IonApp, IonRouterOutlet, setupIonicReact } from '@ionic/react';
import { IonReactRouter } from '@ionic/react-router';
import Home from './pages/Home';

/* Core CSS required for Ionic components to work properly */
import '@ionic/react/css/core.css';

/* Basic CSS for apps built with Ionic */
import '@ionic/react/css/normalize.css';
import '@ionic/react/css/structure.css';
import '@ionic/react/css/typography.css';

/* Optional CSS utils that can be commented out */
import '@ionic/react/css/padding.css';
import '@ionic/react/css/float-elements.css';
import '@ionic/react/css/text-alignment.css';
import '@ionic/react/css/text-transformation.css';
import '@ionic/react/css/flex-utils.css';
import '@ionic/react/css/display.css';

/**
 * Ionic Dark Mode
 * -----------------------------------------------------
 * For more info, please see:
 * https://ionicframework.com/docs/theming/dark-mode
 */

/* import '@ionic/react/css/palettes/dark.always.css'; */
/* import '@ionic/react/css/palettes/dark.class.css'; */
import '@ionic/react/css/palettes/dark.system.css';

/* Theme variables */
import './theme/variables.css';
import { useEffect, useState } from 'react';
import serverService from './services/serverServices';
import { setPrinterServiceBaseUrl } from './services/printerServices';

setupIonicReact();

const App: React.FC = () => {

  const [serverStatus, setServerStatus] = useState<'online' | 'offline'>('offline');
  const [printerServiceReady, setPrinterServiceReady] = useState<boolean>(false);
  const [backendInfo, setBackendInfo] = useState<{ hostname: string; ip_address: string; port: string } | null>(null);

  const handleServerStatusChange = (status: 'online' | 'offline') => {
    setServerStatus(status);
  };

  const handleBackendAddress = async () => {
    try {
      const response = await serverService.getBackendAddress();
      if (response.success) {
        console.log(response.data)
        setPrinterServiceBaseUrl('http://' + response.data.ip_address + ':' + response.data.port)
        setBackendInfo(response.data);
        setPrinterServiceReady(true);
      }
    }
    catch (error) {
      console.error('Error handling backend address:', error);
      setPrinterServiceReady(false);
      setBackendInfo(null);
    }
    finally {
    }
  }

  useEffect(() => {
    if (serverStatus === 'online') {
      handleBackendAddress();
    } else {
      setPrinterServiceReady(false);
      setBackendInfo(null);
    }
  }, [serverStatus])

  return (
    <IonApp>
      <IonReactRouter>
        <IonRouterOutlet>
          <Route exact path="/home">
            <Home
              handleServerStatusChange={handleServerStatusChange}
              serverStatus={serverStatus}
              printerServiceReady={printerServiceReady}
              backendInfo={backendInfo}
            />
          </Route>
          <Route exact path="/">
            <Redirect to="/home" />
          </Route>
        </IonRouterOutlet>
      </IonReactRouter>
    </IonApp>
  )
};

export default App;

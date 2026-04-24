import { IonButton, IonContent, IonHeader, IonIcon, IonPage, IonText, IonTitle, IonToolbar } from '@ionic/react';
import './Home.css';
import PDFAnalyzerUpload from '../components/PDFAnalyzerUpload';
import PDFAnalyzerFiles from './PDFAnalyzerFiles';
import { document } from 'ionicons/icons';

const Home: React.FC = () => {
  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle slot="start"><IonIcon size='small' style={{
                      color: '#3554dcff',
                    }}  icon={document}/> PDF Analyzer</IonTitle>
          <IonButton routerLink='/uploaded-files' style={{padding: 5}} color={'#3554dcff'} slot='end' fill='clear'>Uploaded files</IonButton>
        </IonToolbar>
      </IonHeader>
      <IonContent fullscreen>
        <IonHeader collapse="condense">
          <IonToolbar>
            <IonTitle slot="start"><IonIcon size='small' style={{
                      color: '#3554dcff',
                    }}  icon={document}/> PDF Analyzer</IonTitle>
            <IonButton routerLink='/uploaded-files' style={{padding: 5}} color={'#3554dcff'} slot='end' fill='clear'>Uploaded files</IonButton>
          </IonToolbar>
        </IonHeader>
        {/* <PDFAnalyzerFiles /> */}
        <PDFAnalyzerUpload />
      </IonContent>
    </IonPage>
  );
};

export default Home;

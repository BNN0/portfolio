import { IonAlert, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonGrid, IonIcon, IonRow, IonText } from '@ionic/react';
import './NetworkInfoContainer.css';
import { alert, clipboard, cloud, colorFill, pin, server, warning, wifi } from 'ionicons/icons';
import { printerService } from '../../services/printerServices';
import { useEffect, useState } from 'react';

interface NetworkInfoProps {
    serverStatus?: 'online' | 'offline';
    backendInfo: { hostname: string; ip_address: string; port: string } | null;
}

const NetworkInfoContainer: React.FC<NetworkInfoProps> = ({ serverStatus, backendInfo }) => {



    return (
        <IonCard>
            <IonCardContent>
                {serverStatus === 'online' && backendInfo ?
                    (
                        <>
                            <IonGrid>
                                <IonRow>
                                    <IonCol>
                                        <h2 style={{ color: '#ffff', fontWeight: 'bold' }}>Información de Red</h2>
                                        <p>Detalles de conexión para clientes externos.</p>
                                    </IonCol>
                                    <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'end' }}>
                                        <IonButton disabled style={{ '--background': '#5677b467', opacity: 1 }} shape='round'>
                                            <IonIcon slot='icon-only' icon={wifi} size='large' style={{ color: '#5c90f0', padding: '3px' }}></IonIcon>
                                        </IonButton>
                                    </IonCol>
                                </IonRow>
                                <IonRow>
                                    <IonCol size='12'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonRow>
                                                    <IonCol>
                                                        <IonText style={{ fontSize: '10px' }}>DIRECCIÓN API PÚBLICA</IonText>
                                                        <IonText color={'primary'} style={{ display: 'block', fontWeight: 'bold', fontSize: '26px' }}>{backendInfo?.ip_address}:{backendInfo?.port}</IonText>
                                                    </IonCol>
                                                    <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'end' }}>
                                                        <IonChip
                                                            style={{ fontSize: '10px', marginTop: '10px', cursor: 'pointer' }}
                                                            color={'warning'}
                                                            onClick={() => {
                                                                if (backendInfo) {
                                                                    const apiAddress = `${backendInfo.ip_address}:${backendInfo.port}`;
                                                                    navigator.clipboard.writeText(apiAddress);
                                                                    window.alert('Dirección API copiada al portapapeles: ' + apiAddress);
                                                                }
                                                            }}
                                                        >
                                                            Copiar <IonIcon icon={clipboard}></IonIcon>
                                                        </IonChip>
                                                    </IonCol>
                                                </IonRow>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                </IonRow>
                                <IonRow>
                                    <IonCol size='6'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonIcon icon={cloud} />
                                                <IonText> Hostname</IonText>
                                                <IonText style={{ display: 'block', fontWeight: 'bold', fontSize: '16px', color: '#ffff' }} >{backendInfo?.hostname}</IonText>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                    <IonCol size='6'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonIcon icon={pin} />
                                                <IonText> Puerto API</IonText>
                                                <IonText style={{ display: 'block', fontWeight: 'bold', fontSize: '16px', color: '#ffff' }} >{backendInfo?.port}</IonText>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                </IonRow>
                            </IonGrid>
                        </>
                    )
                    :
                    (
                        <>
                            <IonGrid>
                                <IonRow>
                                    <IonCol>
                                        <h2 style={{ color: '#ffff', fontWeight: 'bold' }}>Información de Red</h2>
                                        <p>Detalles de conexión para clientes externos.</p>
                                    </IonCol>
                                    <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'end' }}>
                                        <IonButton disabled style={{ '--background': '#5677b467', opacity: 1 }} shape='round'>
                                            <IonIcon slot='icon-only' icon={alert} size='large' style={{ color: '#5c90f0', padding: '3px' }}></IonIcon>
                                        </IonButton>
                                    </IonCol>
                                </IonRow>
                                <IonRow>
                                    <IonCol size='12'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonRow>
                                                    <IonCol>
                                                        <IonText style={{ fontSize: '10px' }}>DIRECCIÓN API PÚBLICA</IonText>
                                                        <IonText style={{ display: 'block', fontWeight: 'bold', fontSize: '26px' }}>http://0.0.0.0:0000</IonText>
                                                    </IonCol>
                                                    <IonCol size='3' style={{ display: 'flex', alignItems: 'center', justifyContent: 'end' }}>
                                                        <IonChip disabled style={{ fontSize: '10px', marginTop: '10px' }} color={'warning'}>Copiar <IonIcon icon={clipboard}></IonIcon></IonChip>
                                                    </IonCol>
                                                </IonRow>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                </IonRow>
                                <IonRow>
                                    <IonCol size='6'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonIcon icon={cloud} />
                                                <IonText> Hostname</IonText>
                                                <IonText style={{ display: 'block', fontWeight: 'bold', fontSize: '16px', color: '#ffff' }}>#########</IonText>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                    <IonCol size='6'>
                                        <IonCard style={{ backgroundColor: '#0d131873', border: '1px solid #233341' }}>
                                            <IonCardContent >
                                                <IonIcon icon={pin} />
                                                <IonText> Puerto API</IonText>
                                                <IonText style={{ display: 'block', fontWeight: 'bold', fontSize: '16px', color: '#ffff' }} >####</IonText>
                                            </IonCardContent>
                                        </IonCard>
                                    </IonCol>
                                </IonRow>
                            </IonGrid>
                        </>
                    )
                }
            </IonCardContent>
        </IonCard>
    );
};

export default NetworkInfoContainer;

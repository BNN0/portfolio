// FileListItemComponent.tsx - VERSIÓN LIMPIA Y COMPLETA

import React from 'react';
import {
    IonButton,
    IonIcon,
    IonChip,
    IonSpinner,
    IonRow,
    IonCol,
    IonGrid,
    IonText,
    IonCard,
    IonCardContent
} from '@ionic/react';
import {
    checkmarkCircle,
    timeOutline,
    warningOutline,
    closeOutline,
    trashBinOutline,
    chatboxEllipsesOutline,
    cloudDownloadOutline,
    gitCompareOutline
} from 'ionicons/icons';
import './FileListItemComponent.css'
import useFileStatusPolling from '../hooks/useFileStatusPolling';

interface FileData {
    file_name: string;
    object_name: string;
    size_bytes: number;
    last_modified: string;
    etag: string;
    status: string;
    has_modified: number;
    is_split?: boolean;
    part1_name?: string;
    part2_name?: string;
}

interface FileListItemProps {
    file: FileData;
    downloadingFile: string | null;
    bucketName: string;
    bucketModifyName: string;
    onAnalyzeFile: (fileName: string, objectName: string) => Promise<void>;
    onDownloadFile: (objectName: string, bucketName: string) => Promise<void>;
    onDeleteFile: (objectName: string, hasModified: number) => Promise<void>;
    isProcessing: boolean;
}

const FileListItem: React.FC<FileListItemProps> = ({
    file,
    downloadingFile,
    bucketName,
    bucketModifyName,
    onAnalyzeFile,
    onDownloadFile,
    onDeleteFile,
    isProcessing
}) => {
    const { status: polledStatus, hasModified: polledHasModified, isLoading: isPolling } =
        useFileStatusPolling(
            file.object_name,
            bucketName,
            isProcessing && !['fixed', 'no_changes', 'error', 'split', 'complete'].includes(file.status),
            file.status
        );

    const effectiveStatus = React.useMemo(() => {
        if (!isProcessing || ['fixed', 'no_changes', 'error', 'split', 'complete'].includes(file.status)) {
            return file.status;
        }
        if (isPolling || polledStatus === 'pending') return 'analyzing';
        return polledStatus;
    }, [isProcessing, isPolling, file.status, polledStatus]);

    const effectiveHasModified = React.useMemo(() => {
        return isProcessing ? polledHasModified : file.has_modified;
    }, [isProcessing, polledHasModified, file.has_modified]);

    const isSplit = file.is_split || (effectiveStatus === 'split');

    const getStatusColor = (status: string): string => {
        const colors: Record<string, string> = {
            'complete': '#28a745',
            'fixed': '#28a745',
            'no_changes': '#28a745',
            'split': '#9c27b0',
            'pending': '#007bff',
            'issues': '#ffc107',
            'error': '#dc3545',
            'analyzing': '#17a2b8',
            'processing': '#17a2b8',
        };
        return colors[status] || '#6c757d';
    };

    const getStatusIcon = (status: string) => {
        const icons: Record<string, any> = {
            'complete': checkmarkCircle,
            'fixed': checkmarkCircle,
            'no_changes': checkmarkCircle,
            'split': gitCompareOutline,
            'pending': timeOutline,
            'issues': warningOutline,
            'error': closeOutline,
            'analyzing': timeOutline,
            'processing': timeOutline,
        };
        return icons[status] || timeOutline;
    };

    const getStatusText = (status: string): string => {
        const texts: Record<string, string> = {
            'complete': 'Complete',
            'pending': 'Pending',
            'fixed': 'Fixed',
            'no_changes': 'No Changes',
            'split': 'Split in 2 parts',
            'issues': 'Issues Found',
            'error': 'Error',
            'analyzing': isPolling ? 'Analyzing...' : 'Analyzing',
            'processing': 'Processing...',
        };
        return texts[status] || 'Unknown';
    };

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const formatDate = (dateString: string): string => {
        try {
            return new Date(dateString).toLocaleDateString();
        } catch {
            return dateString;
        }
    };

    const getFileName = (objectName: string): string => {
        const parts = objectName.split('_');
        if (parts.length >= 3) {
            return parts.slice(2).join('_');
        }
        return objectName;
    };

    const shouldDisableAnalyze = isProcessing ||
        ['analyzing', 'processing', 'fixed', 'no_changes', 'split'].includes(effectiveStatus);

    return (
        <IonCard 
            style={{
                margin: '8px 0',
                borderRadius: '8px',
                transition: 'all 0.2s ease',
                border: isProcessing ? '2px solid #17a2b8' : 'none',
                backgroundColor: isSplit ? '#f3e5f5' : undefined
            }}
            onMouseEnter={(e) => {
                e.currentTarget.style.boxShadow = '0 8px 8px rgba(0, 0, 0, 0.1)';
                e.currentTarget.style.transform = 'translateY(-1px)';
            }}
            onMouseLeave={(e) => {
                e.currentTarget.style.boxShadow = '0 2px 10px rgba(0,0,0,0.08)';
                e.currentTarget.style.transform = 'translateY(0)';
            }}
        >
            <IonCardContent style={{ padding: '16px' }}>
                <IonGrid style={{ padding: 0 }}>
                    {/* Filename Row */}
                    <IonRow>
                        <IonCol size="auto" style={{ minWidth: '200px', flex: 1 }}>
                            <IonText>
                                <p style={{
                                    fontSize: '14px',
                                    color: '#333',
                                    fontWeight: '500',
                                    margin: 0,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis',
                                    whiteSpace: 'nowrap'
                                }} title={getFileName(file.file_name)}>
                                    {getFileName(file.file_name)}
                                </p>
                            </IonText>
                        </IonCol>
                    </IonRow>

                    {/* Info Row */}
                    <IonRow style={{ alignItems: 'center', justifyContent: 'space-between', marginTop: '8px' }}>
                        {/* Chat Button */}
                        <IonCol size="auto">
                            <IonButton fill='clear' shape='round'>
                                <IonIcon slot='icon-only' size='large' icon={chatboxEllipsesOutline} 
                                    style={{ color: '#6c757d' }} />
                            </IonButton>
                        </IonCol>

                        {/* Analyze Button */}
                        <IonCol size="auto">
                            {effectiveStatus === "pending" && (
                                <IonButton onClick={async () => await onAnalyzeFile(file.file_name, file.object_name)}
                                    fill="outline" disabled={shouldDisableAnalyze}>
                                    {isProcessing ? (
                                        <>
                                            <IonSpinner name="crescent" style={{ width: '14px', height: '14px', marginRight: '4px' }} />
                                            Starting...
                                        </>
                                    ) : "Analyze"}
                                </IonButton>
                            )}
                        </IonCol>

                        {/* Size */}
                        <IonCol size="auto">
                            <IonText color="medium">
                                <p style={{ fontSize: '14px', margin: 0 }}>
                                    {formatFileSize(file.size_bytes)}
                                </p>
                            </IonText>
                        </IonCol>

                        {/* Date */}
                        <IonCol size="auto">
                            <IonText color="medium">
                                <p style={{ fontSize: '14px', margin: 0 }}>
                                    {formatDate(file.last_modified)}
                                </p>
                            </IonText>
                        </IonCol>

                        {/* Status Chip */}
                        <IonCol size="auto">
                            <IonChip style={{
                                '--background': getStatusColor(effectiveStatus),
                                '--color': 'white',
                                fontSize: '12px',
                                fontWeight: '500',
                                height: '24px',
                                borderRadius: '12px',
                                margin: 0
                            } as React.CSSProperties}>
                                <IonIcon icon={getStatusIcon(effectiveStatus)} 
                                    style={{ marginRight: '4px', fontSize: '14px' }} />
                                {getStatusText(effectiveStatus)}
                            </IonChip>
                        </IonCol>

                        {/* Download/Action Buttons */}
                        <IonCol size="auto">
                            <IonRow style={{ alignItems: 'center', gap: '8px' }}>
                                {effectiveHasModified === 1 && (
                                    <>
                                        {isSplit ? (
                                            <>
                                                {/* Part 1 Download */}
                                                <IonButton fill="outline" color="success" size='small'
                                                    disabled={downloadingFile === file.part1_name}
                                                    onClick={() => onDownloadFile(file.part1_name!, bucketModifyName)}
                                                    style={{
                                                        fontSize: '10px',
                                                        margin: 0,
                                                        '--padding-start': '3px',
                                                        '--padding-end': '3px',
                                                        '--padding-top': '0',
                                                        '--padding-bottom': '0'
                                                    } as React.CSSProperties}>
                                                    {downloadingFile === file.part1_name ? (
                                                        <IonSpinner name="crescent" 
                                                            style={{ width: '12px', height: '12px', marginRight: '4px' }} />
                                                    ) : (
                                                        <IonIcon icon={cloudDownloadOutline} style={{ marginRight: '4px' }} />
                                                    )}
                                                    Part 1
                                                </IonButton>

                                                {/* Part 2 Download */}
                                                <IonButton fill="outline" color="success" size='small'
                                                    disabled={downloadingFile === file.part2_name}
                                                    onClick={() => onDownloadFile(file.part2_name!, bucketModifyName)}
                                                    style={{
                                                        fontSize: '10px',
                                                        margin: 0,
                                                        '--padding-start': '3px',
                                                        '--padding-end': '3px',
                                                        '--padding-top': '0',
                                                        '--padding-bottom': '0'
                                                    } as React.CSSProperties}>
                                                    {downloadingFile === file.part2_name ? (
                                                        <IonSpinner name="crescent" 
                                                            style={{ width: '12px', height: '12px', marginRight: '4px' }} />
                                                    ) : (
                                                        <IonIcon icon={cloudDownloadOutline} style={{ marginRight: '4px' }} />
                                                    )}
                                                    Part 2
                                                </IonButton>
                                            </>
                                        ) : (
                                            /* Single File Download */
                                            <IonButton fill="outline" color="tertiary" size='large'
                                                disabled={downloadingFile === file.object_name}
                                                onClick={() => onDownloadFile(file.object_name, bucketModifyName)}
                                                style={{
                                                    fontSize: '10px',
                                                    margin: 0,
                                                    '--padding-start': '3px',
                                                    '--padding-end': '3px',
                                                    '--padding-top': '0',
                                                    '--padding-bottom': '0'
                                                } as React.CSSProperties}>
                                                {downloadingFile === file.object_name ? (
                                                    <IonSpinner name="crescent" 
                                                        style={{ width: '14px', height: '14px', marginRight: '4px' }} />
                                                ) : (
                                                    <IonIcon icon={cloudDownloadOutline} style={{ marginRight: '4px' }} />
                                                )}
                                                Download Fixed
                                            </IonButton>
                                        )}
                                    </>
                                )}

                                {/* Delete Button */}
                                <IonButton onClick={() => onDeleteFile(file.object_name, file.has_modified)}
                                    color="danger" fill="clear" style={{ margin: 0 }} disabled={isProcessing}>
                                    <IonIcon icon={trashBinOutline} size="small" />
                                </IonButton>
                            </IonRow>
                        </IonCol>
                    </IonRow>
                </IonGrid>
            </IonCardContent>
        </IonCard>
    );
};

export default FileListItem;
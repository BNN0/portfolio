#!/bin/bash

INSTANCE_ID=$(jq -r '.ec2_instance_id.value' terraform_outputs.json)
echo "ID de instancia: $INSTANCE_ID"

if [ -z "$INSTANCE_ID" ]; then
    echo "Error: No se pudo obtener el ID de instancia"
    exit 1
fi

echo "Comando a ejecutar: $COMMAND"

COMMAND_OUTPUT=$(aws ssm send-command \
    --instance-id "$INSTANCE_ID" \
    --document-name "AWS-RunShellScript" \
    --parameters "$COMMAND" \
    --output json)

if [ $? -ne 0 ]; then
    echo "Error al enviar el comando SSM"
    echo "$COMMAND_OUTPUT"
    exit 1
fi

COMMAND_ID=$(echo "$COMMAND_OUTPUT" | jq -r '.Command.CommandId')
echo "Command ID: $COMMAND_ID"

echo "Esperando que el comando termine..."
while true; do
    INVOCATION_OUTPUT=$(aws ssm get-command-invocation \
        --instance-id "$INSTANCE_ID" \
        --command-id "$COMMAND_ID" \
        --output json)
    
    STATUS=$(echo "$INVOCATION_OUTPUT" | jq -r '.Status')
    echo "Estado actual: $STATUS"
    
    if [ "$STATUS" != "InProgress" ]; then
        break
    fi
    sleep 5
done

COMMAND_EXIT_CODE=$(echo "$INVOCATION_OUTPUT" | jq -r '.ResponseCode')

if [ "$COMMAND_EXIT_CODE" -eq 0 ]; then
    echo "El comando se ejecutó exitosamente en la instancia ${INSTANCE_ID}"
    echo "Salida del comando:"
    echo "$INVOCATION_OUTPUT" | jq -r '.StandardOutputContent'
    exit 0
else
    echo "El comando falló en la instancia ${INSTANCE_ID} con estado: $COMMAND_EXIT_CODE"
    echo "Detalles del error:"
    echo "$INVOCATION_OUTPUT" | jq -r '.StandardErrorContent'
    echo "Salida del comando:"
    echo "$INVOCATION_OUTPUT" | jq -r '.StandardOutputContent'
    exit 1
fi
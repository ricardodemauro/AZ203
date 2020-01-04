# Prepacao antes de executar

Criar arquivo `azureauth.properties` na raiz do projeto com o seguinte conteúdo.

```
subscription=<subscription-id>
client=<application-id>
key=<authentication-key>
tenant=<tenant-id>
managementURI=https://management.core.windows.net/
baseURL=https://management.azure.com/
authURL=https://login.windows.net/
graphURL=https://graph.windows.net/
```

Substituir 

- <subscription-id>
- <application-id>
- <authentication-key>
- <tenant-id>


## Referencias

> https://docs.microsoft.com/en-us/azure/virtual-machines/windows/csharp
> https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal
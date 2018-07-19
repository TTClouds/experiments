# Creación de un cluster kubernetes en Azure

## Herramienta de gestión de Azure

* Primero hay que instalar la herramienta de [azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)

* Logarse a la cuenta azure correspondiente:
```
> az login
```

* El documento json devuelto indica las diferentes subscripciones a las que tiene acceso desde su cuenta. Si hay mas de una, hay que comprobar que está seleccionada la correcta. El siguiente commando permite indicar que se desea trabajar con la subscripción de MantTest:
```
> az account set --subscription MantTest
```

* Para trabajar con kubernetes, se debe tener acceso a un registro de contenedores (público o privado) y a un cluster de kubernetes. Para comprobar si en azure contamos con un registro (Azure Container Registry [ACR]) y con algún clúster de kubernetes (Azure Kubernetes Service [AKS]), se emiten los siguientes comandos:
```
> az acr list -o table
> az aks list -o table
```
Por ejemplo
```
NAME            RESOURCE GROUP    LOCATION    SKU    LOGIN SERVER               CREATION DATE         ADMIN ENABLED
--------------  ----------------  ----------  -----  -------------------------  --------------------  ---------------
testmtregistry  testkubegroup    westeurope  Basic  testmtregistry.azurecr.io  2018-06-07T08:29:40Z
```

## Creación del Registro de Contenedores privado

* Todo recurso en Azure se asocia a un grupo de recursos. Para crear un grupo de recursos se utiliza el siguiente comando:
```
> az group create --name <nombre> --location <location>
```
Por ejemplo:
```
> az group create --name testkubegroup --location westeurope -o table
```

* Ahora, para crear el registro de contenedores, se utiliza el siguiente comando. Por ejemplo:
```
> az acr create --name testmtregistry -g testkubegroup --location westeurope --sku Basic --admin-enabled true -o table
NAME            RESOURCE GROUP    LOCATION    SKU    LOGIN SERVER               CREATION DATE         ADMIN ENABLED
--------------  ----------------  ----------  -----  -------------------------  --------------------  ---------------
testmtregistry  testkubegroup    westeurope  Basic  testmtregistry.azurecr.io  2018-06-07T08:29:40Z
```

* Por último hay configurar docker para que utilice el registro privado que hemos creado. Esto permite subir imágenes de nuestro código al repositorio privado en lugar de al público.
```
> az acr login -n testmtregistry -u testmtregistry -p XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

[Para más información](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-azure-cli)

## Creación del Cluster de Kubernetes

[Walkthrough online](https://docs.microsoft.com/en-us/azure/aks/kubernetes-walkthrough)

* Para crear un cluster de k8s con un único nodo del tipo Standard_DS1_v2 (1 vCPU & 2 Gb RAM) con
```
az aks create -n testmtcluster -g testkubegroup -l westeurope --node-count 1 --node-vm-size Standard_DS1_v2 --generate-ssh-keys --node-osdisk-size 50
```

* Una vez creado el cluster (puede tardar unos minutos) se debe conectar el cliente de kubernetes al mismo. El siguiente comando permite hacerlo con facilidad, en lugar de utilizar kubectl.
```
az aks get-credentials -n testmtcluster -g testkubegroup
```

* Para visualizar el dashboard de k8s, lanzar el siguiente comando para permitir al dashboard acceder a la información del cluster:
```
kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard
```
* Y a continuación lanzar el dashboard:
```
az aks browse -n testmtcluster -g testkubegroup
```

## Comandos y referencias útiles

* Para obtener la lista de ubicaciones de data center de azure, utilizar el siguiente comando:
```
az account list-locations -o table
```

* Para mirar los precios de VMs de linux ir a [Linux Virtual Machines Pricing](https://azure.microsoft.com/en-us/pricing/details/virtual-machines/linux/)
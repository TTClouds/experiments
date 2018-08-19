az group create --name testkubegroup --location westeurope -o table
az acr create --name testmtregistry -g testkubegroup --location westeurope --sku Basic --admin-enabled true -o table
az acr credential show --name testmtregistry -g testkubegroup --query passwords[0].value
az acr login -n testmtregistry -u testmtregistry -p XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
az aks create -n testmtcluster -g testkubegroup -l westeurope --node-count 1 --node-vm-size Standard_DS1_v2 --generate-ssh-keys --node-osdisk-size 50 --kubernetes-version 1.10.6
az aks get-credentials -n testmtcluster -g testkubegroup
kubectl create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard
az aks browse -n testmtcluster -g testkubegroup

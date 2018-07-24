kubectl delete deployment tiller-deploy --namespace=kube-system
kubectl delete service tiller-deploy --namespace=kube-system
kubectl delete -f helm-rbac-config.yaml
rimraf C:\Users\isierra\.helm
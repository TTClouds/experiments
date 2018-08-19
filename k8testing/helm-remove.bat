helm reset --remove-helm-home
REM kubectl delete deployment tiller-deploy --namespace=kube-system
REM kubectl delete service tiller-deploy --namespace=kube-system
REM kubectl delete -f helm-rbac-config.yaml
REM rimraf C:\Users\isierra\.helm
kubectl apply -f helm-rbac-config.yaml
helm init --service-account tiller
helm repo add incubator http://storage.googleapis.com/kubernetes-charts-incubator
helm repo add bitnami https://charts.bitnami.com/bitnami
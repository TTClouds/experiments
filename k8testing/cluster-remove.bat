az group delete --name testkubegroup --yes
kubectl config unset users.clusterUser_testkubegroup_testmtcluster
kubectl config unset clusters.testmtcluster
kubectl config unset contexts.testmtcluster

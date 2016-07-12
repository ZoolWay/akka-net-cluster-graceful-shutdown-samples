# akka-net-cluster-graceful-shutdown-samples
Shows a seed node and some workers which should gracefully leave the cluster when terminating:

* a console app (node1)
* an ASP.NET core app (node2)
* a TopShelf hosted app (node3)

They do nothing but join the cluster and leave it when terminating as intented.

Currently using Akka.Cluster 1.1.0.

The samples use log4net and Wire serialization.

Solution file is currently inside of ClusterNode1... could be cleaned up.

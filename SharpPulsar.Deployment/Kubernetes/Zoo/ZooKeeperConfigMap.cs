using k8s.Models;
using System.Collections.Generic;
using System.IO;

namespace SharpPulsar.Deployment.Kubernetes.Bookie
{
    internal class ZooKeeperConfigMap
    {
        private readonly ConfigMap _config;
        internal ZooKeeperConfigMap(ConfigMap config)
        {
            _config = config;
        } 
        public RunResult GenZkConf(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-genzkconf-configmap", Values.Namespace)
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName }
                            })
                .Data(new Dictionary<string, string>
                        {
                           
                            { "gen-zk-conf.sh", @"#!/bin/bash 
        "
    + "# Apply env variables to config file and start the regular command " +
    " "

+"CONF_FILE=$1" +
" "
    +"IDX=$2 " +
    " "
    +"PEER_TYPE=$3" +
    " "

    +"if [ $? != 0 ]; then" +
    " "
        +@"echo ""Error: Failed to apply changes to config file""
 "
        +"exit 1" +
        " "
    +"fi" +
    " "

    +"DOMAIN=`hostname -d`" +
    " "


+"# Generate list of servers and detect the current server ID," +
" "
    +"# based on the hostname" +
    " "
    +"((IDX++))" +
    " "
    +@"for SERVER in $(echo $ZOOKEEPER_SERVERS | tr "","" ""\n"")
 "
    +"do" +
    " "
       + @"echo ""server.$IDX=$SERVER.$DOMAIN:2888:3888:${PEER_TYPE};2181"" >> $CONF_FILE
 "
        +@"if [ ""$HOSTNAME"" == ""$SERVER"" ]; then
 "
            +"MY_ID=$IDX" +
            " "
           + @"echo ""Current server id $MY_ID""
 "
        +"fi" +
        " "
        +"((IDX++))" +
        " " +
        " "
    +"done" +
    " "

    +"# For ZooKeeper container we need to initialize the ZK id" +
    " "
    +@"if [ ! -z ""$MY_ID"" ]; then
 "
        +"# Get ZK data dir" +
        " "
        +"DATA_DIR=`grep '^dataDir=' $CONF_FILE | awk -F= '{print $2}'`" +
        " "
        +"if [ ! -e $DATA_DIR/myid ]; then" +
        " "
            +@"echo ""Creating $DATA_DIR/myid with id = $MY_ID""
 "
            +"mkdir -p $DATA_DIR" +
            " "
            +"echo $MY_ID > $DATA_DIR/myid" +
            ""
        +"fi " +
        " "
    +"fi" +
    ""} 
                });
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
        public RunResult Run(string dryRun = default)
        {
            _config.Builder()
                .Metadata($"{Values.ReleaseName}-{Values.Settings.ZooKeeper.Name}", Values.Namespace)                
                .Labels(new Dictionary<string, string>
                            {
                                {"app", Values.App },
                                {"cluster", Values.Cluster },
                                {"release", Values.ReleaseName },
                                {"component", Values.Settings.ZooKeeper.Name },
                            })
                .Data(Values.ConfigMaps.ZooKeeper);
            return _config.Run(_config.Builder(), Values.Namespace, dryRun);
        }
    }
}

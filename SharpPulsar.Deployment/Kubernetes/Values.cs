using k8s.Models;
using SharpPulsar.Deployment.Kubernetes.Extensions;
using SharpPulsar.Deployment.Kubernetes.Helpers;
using System.Collections.Generic;

namespace SharpPulsar.Deployment.Kubernetes
{
    public class Values
    {
        public Values(string @namespace = "pulsar", string cluster = "pulsar", string releaseName = "pulsar", string app = "pulsar"
            , bool persistence = true, bool localStorage = false, bool antiAffinity = true, bool initialize = true, string configurationStore = ""
            , string configurationStoreMetadataPrefix = "", bool createNamespace = true, string metadataPrefix = "", Authentication authentication = null, Tls tls = null,
            List<string> userProvidedZookeepers = null, Monitoring monitoring = null, Images images = null, Probes probes = null, Ports ports = null,
            ComponentSettings componentSettings = null, ExtraConfigs extraConfigs = null, ResourcesRequests resourcesRequests = null, ConfigMaps configMaps = null,
            Component zooKeeperComponent = null, Component bookKeeperComponent = null, Component autoRecoveryComponent = null, Component brokerComponent = null,
            Component proxyComponent = null, Component prestoCoordinatorComponent = null, Component prestoWorkComponent = null, Component toolSetComponent = null, Component functionComponent = null,
            Component kopComponent = null, Ingress ingress = null, Component prometheus = null, ConfigmapReloads configmapReloads = null, Component grafana = null,
            CertificateSecrets certificateSecrets = null)
        {
            //Testing purposes
            CertificateSecrets = certificateSecrets ?? new CertificateSecrets 
            {
                CertificateAuthority = "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUVERENDQXZTZ0F3SUJBZ0lJTmNPQ1kxdG84SnN3RFFZSktvWklodmNOQVFFTEJRQXdnWXN4Q3pBSkJnTlYKQkFZVEFrNUhNUTB3Q3dZRFZRUUlFd1JQWjNWdU1SRXdEd1lEVlFRSEV3aEJZbVZ2YTNWMFlURVZNQk1HQTFVRQpDaE1NVjJocGRHVWdVSFZ5Y0d4bE1ROHdEUVlEVlFRTEV3WkVaWFpQY0hNeER6QU5CZ05WQkFNVEJuQjFiSE5oCmNqRWhNQjhHQ1NxR1NJYjNEUUVKQVJZU1pXRmlZVzV2Ym5WQVoyMWhhV3d1WTI5dE1CNFhEVEl3TURneU1qRTEKTXpFd01Gb1hEVE13TURneU1qRTFNekV3TUZvd2dZc3hDekFKQmdOVkJBWVRBazVITVEwd0N3WURWUVFJRXdSUApaM1Z1TVJFd0R3WURWUVFIRXdoQlltVnZhM1YwWVRFVk1CTUdBMVVFQ2hNTVYyaHBkR1VnVUhWeWNHeGxNUTh3CkRRWURWUVFMRXdaRVpYWlBjSE14RHpBTkJnTlZCQU1UQm5CMWJITmhjakVoTUI4R0NTcUdTSWIzRFFFSkFSWVMKWldGaVlXNXZiblZBWjIxaGFXd3VZMjl0TUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQwpBUUVBdU9NT3JYY1VlYSs1QXdhQndPL2xqbHkwSC9RdzZKUUNwQklRTGRINWFwa3diVFFWOFJ1WEI5VXRScmNOClErLzFnb1R4dEsyVnBjcGdRcnphNWphdFFNMkpBbWdRSkV6eHc3YWFhVVdKMGJRMys3N2wxaUFYaGVSdWVEakMKZHFRWWNVYXFZYngxaEI4a3l0bkw3Z3UvcXdGa0JmbHIyRFQ0NHVPRXBPeDF6RHB0enpvWUIwdlJZeTV6UnB4Mwp0VUpWZWRhRUwwNm5ES1d4TVIwa3daQWZKWERHOGFiM2tHV3prbnpVY2N3UE9pa2JQdWJVMGNSQ05Jbk1tR1E0Cm8zbUVjVytlYzhWbVp3V2ZRVHNxM1NkbmN6OVdCbzgwVVRLeTdzTmtSWW55VGZRcUN0VFhIN0RnTWNHNEtObWwKZ013ZlJMOXkxMEJRRzNwSGZ6ejBUSHRmblFJREFRQUJvM0l3Y0RBUEJnTlZIUk1CQWY4RUJUQURBUUgvTUIwRwpBMVVkRGdRV0JCUXBTSG9WaEVrU2lVYjBSbGpWMWVjeFFzdllYVEFMQmdOVkhROEVCQU1DQVFZd0VRWUpZSVpJCkFZYjRRZ0VCQkFRREFnQUhNQjRHQ1dDR1NBR0crRUlCRFFRUkZnOTRZMkVnWTJWeWRHbG1hV05oZEdVd0RRWUoKS29aSWh2Y05BUUVMQlFBRGdnRUJBSmdWSjkwQkh4c3pXN1NzSUVWcERNTFhubXYwcFhYcGkyS2hnOXp4L3VYdwpIYTE3WjhYQXlvUWhRSHBpaTUwdkljdGlRK2xZMTZnWUlodWRXVWRGZWx5OWxXU0NVemI5cUJITWhlL2dtcmJ0CjYydUFVWERWTGZhNkwxdE9HekxFYzVsOWphUWhQQUFXMkM0b2Q5T3BJU0hzS3dPRVJldTlIcGw5N3N2dU40cmMKZjdZYnkzSnZmay93dXZGMzdiTGRKWWZxRmdXUzlLSDRDT0NGU3NadFJ0YllvVXhpOTdmWmdlTit2VENGeUE1cgpRTHZ5OWtnbXZKZ2hoMnQ0UkhTSjA4Zkxpc2ZwRTZmMVo5S3VyeFlDY0xibWtuWGUxWVZnV21CZ1h2NTIxcHRjCmw5V1lDL3Ziaml1THEyTUs2a1IrMUsvdUdtOVpOc2RmckRrMHpHdDAwSjQ9Ci0tLS0tRU5EIENFUlRJRklDQVRFLS0tLS0K",
                AzureDnsPassword = "Vnk4dHhUZV80fks2SjA4azV5MWpfay42VDBMSVZqa2dyVQo=",
                Broker = new CertificateSecrets.ComponentSecret
                {
                    Private = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFb2dJQkFBS0NBUUVBd1FndFhncTM1NVFJbTdnWEJ5b3huVDA5STR1eTBzNXFWS0Nrdmo2UEdua0p0c3BxCmg0NnJaNmZycUZwMWhic2xWN1RTNmJjZ1A5UkJpZmVSU1J4M1QzQWxRMFlyRDhaYjl4VElKM3ZSNnZkQkVPajIKME1EdkdUQ3VVRERvdFZRSG5xY2kxdm1XZWt0K2hNK0VtUjVEOGhFbXhqU1VzZkVQNlV2L3RqY1N2djdCa0tWbgpBQ1RMelR1NW1aaE03OHd4T2RpLzZwWTFjUXA5ZjhPcDMreDA2Y2pHVE0reEUwL1pRRWhDZityOXZNNjFqeW5nCm5ZeVp3MXlGanBqZm5vdlpMYk54elgzR09icjJTZlUzT2s3bUdvS1V0bXFGNmRYNGUyK1p1NjhRNE1Ga3BJYzAKMU9MVlJMQ3l1ZEJIdGZQZFo0Y2piRWFCQUF2cjF0elFobEwybndJREFRQUJBb0lCQUZCMHpCUUtlNlMyZXZBTQp4dXhobGRSb0ZmZWUzcVluQ3dMREFtZVpRNlJSRnM5dGh3R0JZY2dPb1piR1BYM0VCMGlMUWUzUU9remdkNEMrCkEzeHB0UUVUU1RURkptV28wK1FrY1ZpanIxYzBQNWFBdzM4M0szRmdiUWRYbTJjWTR5UXBuY2ZrdCtlNVY3NmIKa0RVUi9GZ0U5aGhmQ2lzd3d1VUduRXFpb2RtaE16a0c3OHlybWpqY2N0U1dWaEFYeVJGNENJZWx3am1PZkRKRQpOY00ycVNNaGhEV0QxVmVJcTJGWFVqOEV4SmN2SWdwdGFVeHFyUzYxSE1vbTd0a3E3ZUUrcGNpT2hrNm9ROUVTClU2aDdOTHlrdGRqaEZxNlBpUmRYekkxaUpZWEVVT2hsWktxS0ZuYWhGejlFZlRneW82dS9ZZVRHU3dwcDlxUVMKSEtjNWJzRUNnWUVBOS80Ky9VdEE3UDF4STVtYVB1OVFGUTRLV2U5R2xJQ3BGOWdNcnVLQVdXb0I4N3hMWWxaKwp5VUdUOHhpc3NhRWdzREpPWTJIaUs4WEhYR0liWWFmM2o2Y0hxZ1ZpbFFtMWZLNXBHVm9tdTRWajJKNmVzUGswCkU0N1gydlQ5U0RUR2JIT2xJK3J5OTJKY2diMFFMRTJFRFJCRHFzbmZ1bzhENzkyeWtjWjFsR2tDZ1lFQXgwT28KSHNYOWFOWE5RQnRzRGdSQThjTG9JaUlEakxQTFpQa3JyWUppcUVqQ0VWMVlydVhyMXFoKytFTGFQMmFCdkxLcgpzaEtuclpmbFdjT2FpUVZldWhrMEY4bHFWcGEyVVA4WDNMSlNHbU95eVNrODNCcHlnNEZvbGxxWDl5Z2U1UWRBCkw3cWFNdmtwcWM0ZWE1WXNKcE9XM1dZQ0F1YllFTldvV0dEUXNjY0NnWUJqckwvYWhMV3F5MDcvRlF6SEFONzYKSjNPSHBFR0ZESlZxTFA4a0I0dTQ1SCsyWEZjY1JsR2RTSXRUcVBZNFN0L2RrY3FwN1R5L2hUWFU3dVc4Z1l0aQpKS3RTN2VrcXFBVlhBSzdqYnJXa3B6OXpZSVc5OGR1NWhLOURwVFpzSURJa2d2SzhGZ0hqNXBmeDJYQzNyY3hHCmgrUDZzRHNKTzlSRVE2SXpMMFl4Q1FLQmdBbkRVOXBtSXZ0ZTlsWjh3WGVTVjhoQW4zVUVxNTNhTlUzMk0yQ2wKOGNXREF4Y3N0cXFqRTBJS01XWmlpQ1R4RmN3MENOdUp0SE41N0wvUUtLTXNBeThsQ3Z0YlgvMXNGdlN5K3UxUwpRMW1OcHZYYU1tUXFXNC83NkM3dHMySmxzZFhRM0NFNmlGR1ZDYWlMTTh4YnFFQWZuUld0Nk0xUm1DYURBV09MCklzNkRBb0dBQmVFaVBpL3kzNGlOaHdQMXpoUmJvejBrcG9sU3FaMWFEMWRVeC9ZMTBYQnUzc1g2K3pudW5wWXAKelFwQTZPdzd1dDU5MjRtcFQ3Q25SbXQzenYrK0JJMVNkZjZhNlBpK3RPSzdibnVBSGtpTXJyS0hWanZuSlF4UQpabHdOQksveTRzMVp4ajViNFdScFpjbEppZVowZWV4aUZOUjJmUExiQ3l2RnA4VXpnUEU9Ci0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0tCg==",
                    Public = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUF3UWd0WGdxMzU1UUltN2dYQnlveApuVDA5STR1eTBzNXFWS0Nrdmo2UEdua0p0c3BxaDQ2clo2ZnJxRnAxaGJzbFY3VFM2YmNnUDlSQmlmZVJTUngzClQzQWxRMFlyRDhaYjl4VElKM3ZSNnZkQkVPajIwTUR2R1RDdVVERG90VlFIbnFjaTF2bVdla3QraE0rRW1SNUQKOGhFbXhqU1VzZkVQNlV2L3RqY1N2djdCa0tWbkFDVEx6VHU1bVpoTTc4d3hPZGkvNnBZMWNRcDlmOE9wMyt4MAo2Y2pHVE0reEUwL1pRRWhDZityOXZNNjFqeW5nbll5WncxeUZqcGpmbm92WkxiTnh6WDNHT2JyMlNmVTNPazdtCkdvS1V0bXFGNmRYNGUyK1p1NjhRNE1Ga3BJYzAxT0xWUkxDeXVkQkh0ZlBkWjRjamJFYUJBQXZyMXR6UWhsTDIKbndJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg=="
                },
                Bookie = new CertificateSecrets.ComponentSecret
                {
                    Private = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFb3dJQkFBS0NBUUVBeityc1ZHRkR5bjdIMWRiaDB0WjVodVR0WFR4THJrZm8zaTgzOTBoU2NwMDNkZTgzCmRsNGM0M29KL2hPYWpET1FSMUx3NE1PTVAvSVUxQjQxaUI2blFSbGFVYlYwWis5UXlqUmc3Mjcycmd0ZWJzOFYKL1BOTndkMFJWMm12a3R2Z1RHcG9mdVdlSDdmakFkQXFTM24rYXozN0NFWVhaYUpLbmNDMytPdlcxY09LUnl5YQpjdGw4QlplREZQV2dqbUpyaVd2K3AreUxxWndBZG9aazVKVFRoenNvL2dmUTV5cnBOWXBoZ0FPQlFqbjlDKy9KClNDRGl3azVDQ3A3NUJ1SjI1M0k1ak03SElvNHROdU95RE1UQVlqZ3RiYVJMRmZXNFFsd1ViMmlhNzhib2c1eGwKRWs1elRkQ21yVkVqbjlxbFpuYk9DakhaQWM4VnpSSzN2Y0lHaHdJREFRQUJBb0lCQUFLNXBvMVlsUVhqY1lVWgpBdTY3aHU1QXMrZkQ4WTRBUFVva1BreU1jeUF6ZFZXalpBdTJJaFROTmJKUjJYVzYrMG9rQ1NvNUJlNlVvcmRCCkNoeVZva1dWS256bHJ2ZnB5QnBPWTNMZjluWERpbUpUYm90Wk9ReXdkQmk4TnVQcTQ5NjhpbEFYZkdJWUMrNnIKSnRMRWI1UkNSTGNRRENCMTV4cDRPekVWRGtUUS9FTUJYRWtINkdDOXVtL2dOMVJuY0o4eCtVYTBDQzE4M09WMApjWXBpYSs4a0ZERU5mNW14MlJzWEV5bld6K0F5SnpvOXRaNkZlZ2ZoQ2ZTQnF2dE5YWHh3T0RodFJpOExIazJ2CkpRdlFEY1g3Ym12Umpzd29QMnZnR0VKNVJ4NzFlc01LSVhmUllnSllQWko0WjNmemY3dnUzdFd6UmEvY1p5QW0KTmhXRnJsRUNnWUVBN2RsbVlyZzZEcmVSSGhpNU11bDNjbXA3REk0TmJBclBjMGJ3dzR0cUNtRURDdldIOG9acQpQWmtSRWJCczRxQkNjWUhGaXAyMStXL2RBYzhySU53R2twS3diU2FhOWZ1QlNrcWJJZXFFRXUwNXhLTGZOdWkrCjRjWlNxVGxtT1pyRmdOUkdtVytFd2MzUUlTalg2UGdlMFhMZFU1MUI5dnYxaHprcDcyQmgyVThDZ1lFQTM4akkKZytmZFN5TVVET09haXFwWjFFWm1oZ1FHTXlLREg3ZXR4WFhKMVgrZklpcVE5WkRsKzMvUFhUOWFnU0JSdUNBVwpuZmF1c2hVSGdMMkFnNG5UQTY2TWpldVJndm4xaVpOVzhzcWF6YUh2Rnc5U0tEcWNscXZkUlgyc0V5NFZkS1NqCmJYb0E0UGR2SExORkYvcVgzSGNsQlc1bE1CQ05SSUVJbWZQeVFVa0NnWUJHTUgyeWJFTlZ6SDRhcjVrWG1TWVMKc2JHV1J1VlhHT21YVHp3RnVNS1dSWUtzWFVDOUpVVjg0QXJWZVlib2FmcXhuR1k3UGNkUjBOMGJoNU5tb3dlZApnWVJtOFptUk5hTTExVVpxZjlaeDcyZTR5NGVyb0l1VC9QNnZ5YXlORzB0bGRUOVFVRVNSSExkcTBhN0ZwVk1TCjVCN0VhZ2ZwWnZsUjZtQ2hyNE93cVFLQmdRRFVTMG5OdUx5NmR3Q2lhWmxHU3UwRTcvUjYxbjU3TEJad2xIT0oKaTRCNXhhUlZhVVF6Y2M1N2xIaEg0YjRlR1diczRhUVRIdDREbXVlUFBqY1lranRZbHRKSUlGM2VmdnBzRlJhNwpKWEZOK24weXh4b05oK3pkRXhYS3dybm5TQ1NhajBWcXFmOERiRGhBWmVENktvUytaVmo5bWdqc1hBZG9JWERqCmZBVXA0UUtCZ0hoRmxxdkg0dzJ1Sk1ieEk0Z2FKWElVallmWVloWTk2WHJLSU5RZmtKTmc2akhPSXc4QVYrb0EKRlJYaEh1UDlQclR4UVJlR0FDMlZ4elY0WHlwTllFVk4xbXJZYVY3ellxL0N4V2lsb2I5a2VURHNWaENBS3N2NAp5VU12STFvZHlKSmF6enlOZFlLbEdEcUx4aVRMKzZZZnh0RDJUa0dUdExwZEZYcXFwRjV2Ci0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0tCg==",
                    Public = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUF6K3JzVkdGRHluN0gxZGJoMHRaNQpodVR0WFR4THJrZm8zaTgzOTBoU2NwMDNkZTgzZGw0YzQzb0ovaE9hakRPUVIxTHc0TU9NUC9JVTFCNDFpQjZuClFSbGFVYlYwWis5UXlqUmc3Mjcycmd0ZWJzOFYvUE5Od2QwUlYybXZrdHZnVEdwb2Z1V2VIN2ZqQWRBcVMzbisKYXozN0NFWVhaYUpLbmNDMytPdlcxY09LUnl5YWN0bDhCWmVERlBXZ2ptSnJpV3YrcCt5THFad0Fkb1prNUpUVApoenNvL2dmUTV5cnBOWXBoZ0FPQlFqbjlDKy9KU0NEaXdrNUNDcDc1QnVKMjUzSTVqTTdISW80dE51T3lETVRBCllqZ3RiYVJMRmZXNFFsd1ViMmlhNzhib2c1eGxFazV6VGRDbXJWRWpuOXFsWm5iT0NqSFpBYzhWelJLM3ZjSUcKaHdJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg=="
                },
                Proxy = new CertificateSecrets.ComponentSecret
                {
                    Public = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUE0MkhGamtwVG5rOVo3QXJoOHpiYwp5OWdVL1lEZk9iT0dvbmZGZU5mUjRjTWxlWWl0Wm5hOFhySCtPN3UzanJlK0gvYlYvTzZJL1ZEeWt4bCtTbTZQCjFMNGJlaTNmMDRtQUxLcXdsbDBISVJiVWxsS202eHprS0lZTnBLdXNjM0tyTzlDVTR4N2FhaU50NXZzWnkvbmgKK3pJdDYwUm80TmR1eWVvRTVnN0czUStkZ0hPQVlVSS9YTzdXc3VhT3hNb2pSOVpMbXUzS2plNXo3ZUU1d1lmdwpRU0NYK2lYc3BGK012T2pkMVlrRDVyUGRVdEZBRGlRRitBbTVqSnI4TE5Oakw0WjdQc2NhdWw1ZGJkRzJnVWhDCnBIZmlpem1zVlZYR1VIdXpCS2RDdXJCZUFNNnVtY0NPZzBpVGppc2c5dCt0M3ByaFQ0MnZUNFBadUJtaVl5c20KS1FJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg==",
                    Private = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFcEFJQkFBS0NBUUVBNDJIRmprcFRuazlaN0FyaDh6YmN5OWdVL1lEZk9iT0dvbmZGZU5mUjRjTWxlWWl0ClpuYThYckgrTzd1M2pyZStIL2JWL082SS9WRHlreGwrU202UDFMNGJlaTNmMDRtQUxLcXdsbDBISVJiVWxsS20KNnh6a0tJWU5wS3VzYzNLck85Q1U0eDdhYWlOdDV2c1p5L25oK3pJdDYwUm80TmR1eWVvRTVnN0czUStkZ0hPQQpZVUkvWE83V3N1YU94TW9qUjlaTG11M0tqZTV6N2VFNXdZZndRU0NYK2lYc3BGK012T2pkMVlrRDVyUGRVdEZBCkRpUUYrQW01akpyOExOTmpMNFo3UHNjYXVsNWRiZEcyZ1VoQ3BIZmlpem1zVlZYR1VIdXpCS2RDdXJCZUFNNnUKbWNDT2cwaVRqaXNnOXQrdDNwcmhUNDJ2VDRQWnVCbWlZeXNtS1FJREFRQUJBb0lCQUU3Y3VESVRvVkRFNE1FMwpQUXFudW9pUWx5Y2RMVTdMN3lRbU9qMGhUVS9wNlBOdjdnUWRwbE9KbEUyUFc2aUtIY3prUlcrR0k4a0g3RG9NCm0zcWhRZzFkS2hhRUZwODlqMUR1bW5Sc0syN1Voa0xrdzdQRHYzWTJtRURHc2ZjUWpFY2ducUx2TG4ybGNCYXUKZkZIOGh0NGlsejZENllRaDgvRGYwM2NmSk9YQ1AxRUdJSXBibm51aXR2alIvbTg4MnZiQXBkWTJHWHIrT2JjVAp2dC8ybyt0SGc2SUNySW9hVURqbkFMUEhDLzdrUW5VeXpYYll0RWVLRE9XZFNNUTMrSWdrcFpLSzJuTHNlMzgxCldFVy9qT2JuK2pCTUI0OUZueDVBUTdxbC9UR2RQNU4wNU5UZ2MvaHo3enVvd3hVMjFsTzN4bVVMcitVQ3JkY0MKNjVrZ0dVa0NnWUVBOWN5QnIvU2p1M2VyeEFJR0FxdTJNSWFoaHpJRXlQcXBNcW9Uay9JdUFIZmdRRlNqMlE4Kwo3LzRrdHF4a2Y1NjRjNjVsN09MazlLRFJkeFlDVlJ2RXN1aWlaR1hKa2oxc3dzemtFU05RaXA1WnhnNHl5RHVECmgwenB5WGxlMjFyL2JmV1FYSmV1TUgwVm5TMXliVTFPUG9XY284OGFZQUpra3BaSTM4cStGYzhDZ1lFQTdOR1kKR2M1bVUrRkl1Rk1kc0haaXZVNTlPWGJQR3dROCt1MGNDVWp5eGFvc1BWSGVSQzd4YXdwcU5zTVBraHpHazExKwowd2JHcUVlc2NMQmZKQ2U3WjNPTG5QTW84MlZJSG5NOHprSzhqMklVR1FReTRuRWRzNVZ6WkFhL1JZWFNUb2VUCjZjay9uRzUrQlM1M3RUMUtOeEs4VHJPc0NsZnRpRTBmZ0hBbmVvY0NnWUVBNnI0RnVRSmRnRU1ZNHBmQUg3clgKdzE1QVUzcGNnWWlLSUYyM1Z0cTZQaTQrRjVIOTdPV1hpT0hoTkNoTnptZko1b2pPeEw0b1JNeDFYMUxBcFlGZwppRENPTG0zYlpQT090RGV6TS83VEE1K1pRd0g2VTZvcXdnT2RYcEd3R3JPOEw4cU90UzhTNXpIK2UxNlU0bmdxCjJxRUY2SmQ0cStwOUhGVzBnUzRCL1pjQ2dZQkl6d081WTJpNGZ6bnhXYlIyRFYrOEhnYUlCVXdWWFU4MlFuSHQKTk4xWEFrUEpzb0xvYVpwVnM0VUdReUJsWnExeW56c2Z2Q1NWbGp1aEJjaXFnQUN4QktnMjM5ZUdSV3hQMkZRbwpnZ0lnL3lGNHMzN0ZlY3VNNi9Ubkd0L3hpdndtb3E1S0lWS0tVTG96ODU1Z1BYQzB6L0RQTFpSR05kUlVwSWw3CjNuOTB2d0tCZ1FDejNOVEx4SDNZZFQrMDFXUFNLRmRCb0tybjZKR1V3bWxYeHN0akRjZkZuT3RuazJXWDdwL2kKeW9xcGhqWnR4bGwrVnNNQkxxUW04QXNVQlk3bXlQTVl5eUJQcVFFWHZUdkxMbDNFdWVIL3c1cC9BOUZyQ00yawpLRXI0R2ZBN3QzN2o3TENKMlpMY3FNaWRlQVZnMG9oUnB1QkREWW1XalFTYkdyYnIvdkhTWFE9PQotLS0tLUVORCBSU0EgUFJJVkFURSBLRVktLS0tLQo="
                },
                Zoo = new CertificateSecrets.ComponentSecret
                {
                    Public = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUFzT25wV1p5RlRPdm16UjJCbk5xOQpNN0NoRVU2TkpHZzJhanZxbjdQaGUxcUNNREtCQUdLcFhiQ3UrVkxzOE5aYUpvYTBuUFFDMVVHU2oybGJJSDJyCkRQZXJQNml5OGQvdkltQmpGK0h3UFhSWGEzQytmZzk0dFJrcmlvb3hhYXhrbmNGYkh1K0F6UkI1S1dzV2ZaczcKbitXLys2ZDhhOEpyQWNxYUlkSy9zSkx3VTZlMlV3KzdaUjVIZloyLzUvY3d4SFBZTzJSWEFOaklHWFZvcmpsWgpNdTFtT3QxR0hIWEdUeEg4TTBDYnVaZ2QzVVFESDhXc2VnSU1UTUsyTlIyZkViTG1OL0M4TGszNnRsY2xNZXVxClJnVWZNKzhSY09aZWlLMmV4L0xIdGRUejlSanI2Mml5NWRnTWJtL3V1R1NEMjBLdkNOaXVmbU1mVDBmSnNxdXgKU1FJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg==",
                    Private = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFcEFJQkFBS0NBUUVBc09ucFdaeUZUT3ZtelIyQm5OcTlNN0NoRVU2TkpHZzJhanZxbjdQaGUxcUNNREtCCkFHS3BYYkN1K1ZMczhOWmFKb2EwblBRQzFVR1NqMmxiSUgyckRQZXJQNml5OGQvdkltQmpGK0h3UFhSWGEzQysKZmc5NHRSa3Jpb294YWF4a25jRmJIdStBelJCNUtXc1dmWnM3bitXLys2ZDhhOEpyQWNxYUlkSy9zSkx3VTZlMgpVdys3WlI1SGZaMi81L2N3eEhQWU8yUlhBTmpJR1hWb3JqbFpNdTFtT3QxR0hIWEdUeEg4TTBDYnVaZ2QzVVFECkg4V3NlZ0lNVE1LMk5SMmZFYkxtTi9DOExrMzZ0bGNsTWV1cVJnVWZNKzhSY09aZWlLMmV4L0xIdGRUejlSanIKNjJpeTVkZ01ibS91dUdTRDIwS3ZDTml1Zm1NZlQwZkpzcXV4U1FJREFRQUJBb0lCQUZubG9yUHNlemdKTDNEUApLNHVmQTBGKzRYbS81cXkzWDk4L3J3dVVCbUgrTGVWNzVGMWp4UmhjcmF6MzNIck5FV1krVVF6b1dZQXdIOTlMCjlBaGdrMGkxbVlseG1leWFsQk50MHl6Um5KZzl4Q1pPVmg0TWtwUER4SUpUa3FSVis1TEdMQjFlS3A4SlhlZ1UKQ0l1dkUrOCtlZjNRcjhLNk9sSGd0ODIwSDhBRE1jR21TRHFMS2RPV3VzTWxYa0tveUVIWnlNc1BvdWxpSGNXcApXWndtT09taFNUT3k3ZDIyVFBtWm1xbEdSSW5uNGRSV0VxbWNOOGRlajFpdmFhc1BodlJsUGU4T2dGNWRSc0pKCmV4UlZiZUpYY3krSEpha1lsRFN6eWQ4MXZtTklHMjMxNlNIbXVWY1JJR2lwZUpWTjkyZnBuakZzei9rRHU4SkEKdXlNUUMxa0NnWUVBNVlLejFxMk5WY0sveTVMd1hoakhvVXd2OEhxcEpZeUJaVk0wZnBqWWh2ZFZCcXVwOGFiQgpIUHR0ZkZEOEJLeTdNQ1ZBZFgzMHhxZU82Mk1TWWE2K2VtdjZHNGIwLytPaW0vOFBWYVJ5NEFwcDJTL3lHYU4vCjl6WWxzelpCYTRsVmtjU3cwekJxTFBSeGkrdWJpU3lRNlppcjlzdFNSL0xESklEajdtVm9yWk1DZ1lFQXhWVWsKVWNlMDhnTTVUbEVwa09xYkZSR2o5Ynl6SncyeGtFRDJ2UDU4cC93a3NJTi80ZllZc3AwWTVjTjdLN2hJNzZlQQpvZEk1TC9YNk85YmRXQWJRNWdzTnAvMHBFRlpQZzYzeVJhME1DM2ZwQnBha2VwZEh0TWpPZi9pVTQ4RFRsWDQ4CnQ0VHNZeDhXOVdlV1NpaUl2U2s2ZU4vTGE4NEhpVHc0ckt3MGp6TUNnWUVBblltVzJoL3M2Tll5QXBHTTdub0IKNVlTL1QwbFMrNVF6YVpLd2NNbUhyelRzcHhTRUpYeDZCK1BKcGxDTWZNVDRCRGM5eEtnOXNYSm9Wc2g5WUpHcQo2NnRjVlRMUXp5aTRnRzJXWUFudVZEeXhwVVFVNFdacU93MXUyVkcvbkFuN3M0QVloQTR5ZzBNVEFhRXE5UUNqClE4VHBIUkU1SEd2VmFTUVQwSnJKUElNQ2dZQjluUU12bzhiRlA5UWN6SUYvSkRod0l2aVNGdnFiNDVXaVZCZzgKbW1yVHJDZld6UDU4NG1FNllkSlZnQ2hKM2xkZlp1cjFGNU1idXFMOXNIclo3QXpTUXpzQU1xRVBLMElXZW9YOApZSG1JVzE1VXVWUWVUV3B4NUZtL0VJZ0dxdHFGRVFTcGRjM2NFeEJVU3dwYVRvOHNpckFUL2JIS0FDNi81enZVCmZ4SStEUUtCZ1FDT2RrbUFld0NucVhJN1FxaEhmNVZ3SjB0RHROc3F5M1BYQnlMREVrd200M2hxQUhQTFpyYWUKVmhtV2tRdExIOUtvWnU4VnlYSEU4Wm5RM2Y3eU9OUjB4TEVlTVpDVzhqZ1hhMlFZb3F1T2N3dUtjdy9JUHFCNAo3K0tPZW9pdW5oem1JdmU2bjdlM1UrSGN5UmFUMmpTRks4Kzd1dEV4VFpXVERmQklUTkwxeFE9PQotLS0tLUVORCBSU0EgUFJJVkFURSBLRVktLS0tLQo="
                },
                Recovery = new CertificateSecrets.ComponentSecret
                {
                    Private = "LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFcFFJQkFBS0NBUUVBM09ZT0tCUGp1L1NFMzVNL0xManN6WTVDdnhoRHEyeFZEckpaUHZiUzhLKy8vb2dwCk9OOVZ2NnhGMHhCYVN6OEpqRVViNnBZLytNenpkOFJIcUJsclBDRllIZXVnbFF2MmxWNlB4NlVRV3FRZ0Nha1IKK2owUFRMdjd6bkptSWFBZ2NMblZxWWJiQ2ZscTVoMWR6V2tvSjk1TDFuMVZPRmRKVXFwOFVySkdSMU1VRDBnWApxVFh4MUFKQUVKZ1NtMG1DMDUzWExtbDJSTkh2cjR5MDIxUWRKTXl4RlZ3eWxuQmtlYmJPU29ZanJkMTVzVXRYCjdJZmRMVzNCcW1nOXdScWhxRmdlYldwUmpPbTdCTHBES1JCeFBYdVBWd3pEdkV6RVhIcVQweXR1ZUU2OC85MnMKUnI4bWRGTGdmeGI1Qi8zZEc3eW5ubmpGd1FHRFgzNUIvZlI5cHdJREFRQUJBb0lCQVFDcyszYlFpYXJIMEJhUgpZaVNScUFyQWVZdnBTMTRLaXA0ZEVTcjBOS05CR2MvMnliZkdNcFphcjlSS0VUODBONXdKZlVEOE5rYklWZTYrCnRqVUsvVVROWktzMVd3UjRVMUw0NjRFYWJUZGVVN0pHL2wvMm53UXhLZkJwWFlwL0FIOEc1Mi9hL3FEZXNiTzEKbWYrUDBLNUNja0RmS1d5bjN5cXJFcGlpeGJwbE5NTzBUSVordUNYTko0Kzh0SXZaNXVsREdqVGZSZDhlamVpSQp5N0p5Z2k1MlhqWHdLMUN0dTVJR2xlNHRtWUI0RUZkQlA4WHVxYmtXT0MvYVRPS2FXaWhDR3VJcFdreElqaDJ4Cjl3U2p1TDB2d3JnUTJQenExUmlsc0xXdkFjbWFLMlFVWSsrY0ZLbmllQ0V2ZEcweVFDVnRVVlZuTElDOEtlbkQKVDVWZlNKZmhBb0dCQVB0L3pQNWdOMDBOc0lzck1QR0JXaGhIRXJQZHVTYWZ1cEhrSlNZRFFoeFh2QU41eXZBRgphZXhrNXA0MHoyNFcwUEVyTnBXak55Z05jYjF6SS9zT0VkaWdaZUxndEtGUlg0WEpkZ3JrSFdzK3BEaUU3WDNrCjB2TmZxU09KRVpiamdaQlR6anl6WkpqbmtRc2NLRjZseG1OWHo2SGg4YW9qeWRoR2FRWXhFWjlSQW9HQkFPRGEKRUQ0SmZKQS9KdHZDQjYva1FOYUV6ZkN4UXdNd1BnWks5ai9pZFNkcGUyeWlZOWtkajloNlN5c2FtOVptVUtragpnNDVod2x3MGt3eVdzb3BZQm8xd3lHS2JMYUIwb2drT05aczNmOHZUVUlOK0RQS0NjUldvQnRjdXhlNnYxbTBuCjR6aG42UGR2RmxOWjBNWjUzOE81SmNVS3N0Q2w0UFZwUnRVOUJyOTNBb0dCQU1CeGxvYS9VUkdnL0FwQnpuMisKSVJhYXEvRCtKSU10ampHOGhjQ0VsYjNpVkhmRVpra3JtMVhNRDd2WFpUSTBPMFdQYjRFcEZ6ZUtzaEhwWFFycQpSVFdoNXRTb1pROWJtT0JpdS9TeGdPRmpXWDMyR1ZSUUdDc3FjOTVCTURocGRlYmVlZDF3MS9VNG5JQUgxOHcwCnhZMld6OFpyZ2VSUzVreWI5QmxNeXROQkFvR0FaZjlsZU03UzI1aGFKendZUXBqWE5MaWZ0dnlpT25NSzM4M24KY01sb2ZZMWkrTCtkYmFMMFdxMzNKVUYzeWNVMTk5UHRYSXhLSDR1VjNSTUxRS2gzcUhldDN0VW4yRzZ3QmsyVQowYWxXWm42Z09sWFd4N2VXVnMyVzlNdjU2N0dHSXBRQ2hkYlZIbEVkSG9oU3BZWXBsRjZMbkp1aXkvVkRXKy82CnVzWnBKamNDZ1lFQW54QmRXNXFxbmZIMnNqaFZFQ1BtTlhxVmlCMzhXendqK25PNFVxTzlFaVFPRnZZM0U2NVAKL0FzUlJEbFcxb2VhanMwbE8rOUFDeTB1VnR5cFFKU1Y5ZFNwUnBJYmswODZmL1pkWWY0TDRkYllJS2xjSEpxZwpyVDMzbStvSm40TGUrenRiTUt3UFJUbU5jVHh2Nk9sWS9WVlcyNFhRYXdFT21aMzdsVEN5eGZzPQotLS0tLUVORCBSU0EgUFJJVkFURSBLRVktLS0tLQo=",
                    Public = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUEzT1lPS0JQanUvU0UzNU0vTExqcwp6WTVDdnhoRHEyeFZEckpaUHZiUzhLKy8vb2dwT045VnY2eEYweEJhU3o4SmpFVWI2cFkvK016emQ4UkhxQmxyClBDRllIZXVnbFF2MmxWNlB4NlVRV3FRZ0Nha1IrajBQVEx2N3puSm1JYUFnY0xuVnFZYmJDZmxxNWgxZHpXa28KSjk1TDFuMVZPRmRKVXFwOFVySkdSMU1VRDBnWHFUWHgxQUpBRUpnU20wbUMwNTNYTG1sMlJOSHZyNHkwMjFRZApKTXl4RlZ3eWxuQmtlYmJPU29ZanJkMTVzVXRYN0lmZExXM0JxbWc5d1JxaHFGZ2ViV3BSak9tN0JMcERLUkJ4ClBYdVBWd3pEdkV6RVhIcVQweXR1ZUU2OC85MnNScjhtZEZMZ2Z4YjVCLzNkRzd5bm5uakZ3UUdEWDM1Qi9mUjkKcHdJREFRQUIKLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg=="
                }

            };
            ResourcesRequests = resourcesRequests ?? new ResourcesRequests();
            Authentication = authentication ?? new Authentication
            {
                Enabled = false
            };
            Tls = tls ?? new Tls
            {
                Enabled = true
                
            };
            Namespace = @namespace;
            Cluster = cluster;
            ReleaseName = releaseName;
            App = app;
            UserProvidedZookeepers = userProvidedZookeepers ?? new List<string>();
            Persistence = persistence;
            LocalStorage = localStorage;
            AntiAffinity = antiAffinity;
            Initialize = initialize;
            ConfigurationStore = configurationStore;
            ConfigurationStoreMetadataPrefix = configurationStoreMetadataPrefix;
            NamespaceCreate = createNamespace;
            MetadataPrefix = metadataPrefix;
            Monitoring = monitoring ?? new Monitoring();
            Images = images ?? new Images();
            Probe = probes ?? new Probes();
            Ports = ports ?? new Ports();
            Settings = componentSettings ?? new ComponentSettings
            {
                Autorecovery = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 1,
                    Name = "recovery",
                    Service = $"{ReleaseName}-recovery",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-recovery.{Namespace}.svc.cluster.local",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                ZooKeeper = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "zookeeper",
                    Service = $"{ReleaseName}-zookeeper",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-zookeeper.{Namespace}.svc.cluster.local",
                    Storage = new Storage
                    {
                        ClassName = "default",//Each AKS cluster includes four pre-created storage classes(default,azurefile,azurefile-premium,managed-premium)
                        Size = "50Gi"
                    },
                    ZooConnect = Tls.ZooKeeper.Enabled ? $"{ReleaseName}-zookeeper:2281" : $"{ReleaseName}-zookeeper:2181",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "OrderedReady",
                    Resources = new V1ResourceRequirements
                    {
                        Requests = new Dictionary<string, ResourceQuantity>
                                {
                                    {
                                        "memory", new ResourceQuantity(ResourcesRequests.ZooKeeper.Memory)
                                    },
                                    {
                                        "cpu", new ResourceQuantity(ResourcesRequests.ZooKeeper.Cpu)
                                    }
                                }
                    }
                },
                Broker = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "broker",
                    Service = $"{ReleaseName}-broker",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-broker.{Namespace}.svc.cluster.local",
                    ZNode = $"{MetadataPrefix}/loadbalance/brokers/" + "${HOSTNAME}." + $"{ReleaseName}-broker.{Namespace}.svc.cluster.local:2181",
                    UpdateStrategy = "RollingUpdate",
                    EnableFunctionCustomizerRuntime = false,
                    PulsarFunctionsExtraClasspath = "extraLibs",
                    RuntimeCustomizerClassName = "org.apache.pulsar.functions.runtime.kubernetes.BasicKubernetesManifestCustomizer",

                    PodManagementPolicy = "Parallel",
                },
                BookKeeper = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "bookie",
                    Service = $"{ReleaseName}-bookie",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-bookie.{Namespace}.svc.cluster.local",
                    Storage = new Storage
                    {
                        LedgerSize = "50Gi",
                        JournalSize = "10Gi",
                    },
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                Proxy = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 3,
                    Name = "proxy",
                    Service = $"{ReleaseName}-proxy",
                    Host = "${HOSTNAME}." + $"{ReleaseName}-proxy.{Namespace}.svc.cluster.local",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel",
                },
                PrestoCoord = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 1,
                    Name = "presto-coordinator",
                    Service = $"{ReleaseName}-presto-coordinator",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel",
                },
                PrestoWorker = new ComponentSetting
                {
                    Enabled = true,
                    Replicas = 2,
                    Name = "presto-work",
                    Service = $"{ReleaseName}-presto-worker",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel"
                },
                Kop = new ComponentSetting
                {
                    Enabled = false
                },
                Function = new ComponentSetting
                {
                    Enabled = true,
                    Name = "functions-worker"
                },
                AlertManager = new ComponentSetting
                {
                    Name = "alert-manager"
                },
                PulsarDetector = new ComponentSetting(),
                Prometheus = new ComponentSetting
                {
                    Enabled = true,
                    Name = "prometheus",
                    Replicas = 1,
                    Service = $"{ReleaseName}-prometheus",
                    UpdateStrategy = "RollingUpdate",
                    PodManagementPolicy = "Parallel", 
                    Storage = new Storage
                    {
                        Size = "10Gi"
                    }
                },
                Grafana = new ComponentSetting
                {
                    Enabled = true,
                    Name = "grafana",
                    Replicas = 1,
                    GracePeriodSeconds = 30,
                    Resources = new V1ResourceRequirements
                    {
                        Requests = new Dictionary<string, ResourceQuantity>
                        {
                            {"memory", new ResourceQuantity("250Mi") },
                            {"cpu", new ResourceQuantity("0.1") }
                        }
                    }
                }
            };
            ExtraConfigs = extraConfigs ?? new ExtraConfigs
            {
                ZooKeeper = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "ZkServer", new List<string> { } },
                        { "PeerType", "participant" },
                        { "InitialMyId", 0 },
                        { "UseSeparateDiskForTxlog", false },
                        { "Reconfig", false }
                    }
                },
                Broker = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        {"AdvertisedPodIP", false }
                    }
                },
                Bookie  = new ExtraConfig
                {
                    ExtraInitContainers = new List<V1Container>
                    {
                        new V1Container
                        {
                            Name = "wait-zookeeper-ready",
                            Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                            ImagePullPolicy = Images.Bookie.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{string.Join(" ", Args.WaitZooKeeperContainer()) }
                        }
                    },
                    Containers = new List<V1Container>
                    {
                        new V1Container
                        {
                            Name = $"{ReleaseName}-{Settings.BookKeeper.Name}-init",
                            Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                            ImagePullPolicy = Images.Bookie.PullPolicy,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{string.Join(" ", Args.BookieExtraInitContainer()) },
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                    }
                                }
                            }
                        }
                    },
                    Holder = new Dictionary<string, object>
                    {
                        {"RackAware", true }
                    }
                },
                PrestoCoordinator = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "memory", "2G"},{"maxMemory","1GB" },{"maxMemoryPerNode", "128MB"},
                        {"Log", "DEBUG" }, {"maxEntryReadBatchSize", "100"},{ "targetNumSplits", "16"},
                        {"maxSplitMessageQueueSize", "10000"}, {"maxSplitEntryQueueSize", "1000"},
                        {"namespaceDelimiterRewriteEnable", "true" },{ "rewriteNamespaceDelimiter", "/"},
                        {"bookkeeperThrottleValue", "0" }, {"managedLedgerCacheSizeMB", "0"}
                    }
                },
                PrestoWorker = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        { "memory", "2G"},{"maxMemory","1GB" },{"maxMemoryPerNode", "128MB"},
                        {"Log", "DEBUG" }, {"maxEntryReadBatchSize", "100"},{ "targetNumSplits", "16"},
                        {"maxSplitMessageQueueSize", "10000"}, {"maxSplitEntryQueueSize", "1000"},
                        {"namespaceDelimiterRewriteEnable", "true" },{ "rewriteNamespaceDelimiter", "/"},
                        {"bookkeeperThrottleValue", "0" }, {"managedLedgerCacheSizeMB", "0"}
                    }
                },
                Prometheus = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        {"PrometheusArgsRetention", "15d" },
                        {"ConfigmapReload", false },
                        {"ExtraVolumeDirs", null },
                        {"VolumeMounts", new List<V1VolumeMount> ()}
                    }
                },
                Grafana = new ExtraConfig
                {
                    Holder = new Dictionary<string, object>
                    {
                        {"Username","pulsar" },
                        {"Password","pulsar" }
                    }
                }
            };
            ConfigMaps = configMaps ?? new ConfigMaps();
            //Dependencies order
            ZooKeeper = zooKeeperComponent ?? ZooKeeperComponent();
            BookKeeper = bookKeeperComponent ?? BookKeeperComponent();
            AutoRecovery = autoRecoveryComponent ?? AutoRecoveryComponent();
            Broker = brokerComponent ?? BrokerComponent();
            Proxy = proxyComponent ?? ProxyComponent();
            PrestoCoordinator = prestoCoordinatorComponent ?? PrestoCoordinatorComponent();
            PrestoWorker = prestoWorkComponent ?? PrestoWorkComponent();
            Toolset = toolSetComponent ?? new Component();
            Kop = kopComponent ?? new Component();
            Functions = functionComponent ?? new Component();
            Ingress = ingress ?? new Ingress
            {
                Enabled = true,
                Proxy = new Ingress.IngressSetting
                {
                    Enabled = true,
                    Type = "LoadBalancer"
                }
            };
            ConfigmapReloads = configmapReloads ?? new ConfigmapReloads();
            Prometheus = prometheus ?? PrometheusComponent();
            Grafana = grafana ?? GrafanaComponent();

        }

        public static CertificateSecrets CertificateSecrets { get; set; }
        public static Rbac Rbac { get; set; } = new Rbac();
        public static List<string> UserProvidedZookeepers { get; set; }
        public static bool Persistence { get; set; }
        public static bool LocalStorage { get; set; }
        public static bool AntiAffinity { get; set; }
        // Flag to control whether to run initialize job
        public static bool Initialize { get; set; }
        public static string ConfigurationStore { get; set; }
        public static string ConfigurationStoreMetadataPrefix { get; set; }
        //Namespace to deploy pulsar
        public static string Namespace { get; set; }
        public static string Cluster { get; set; }
        public static string ReleaseName { get; set; }
        public static string App { get; set; }
        public static bool NamespaceCreate { get; set; }
        //// Pulsar Metadata Prefix
        ////
        //// By default, pulsar stores all the metadata at root path.
        //// You can configure to have a prefix (e.g. "/my-pulsar-cluster").
        //// If you do so, all the pulsar and bookkeeper metadata will
        //// be stored under the provided path
        public static string MetadataPrefix { get; set; }
        public static ConfigmapReloads ConfigmapReloads { get; set; }
        public static Tls Tls { get; set; }
        //// Monitoring Components
        ////
        //// Control what components of the monitoring stack to deploy for the cluster
        public static Monitoring Monitoring { get; set; }
        //// Images
        ////
        //// Control what images to use for each component
        public static Images Images { get; set; }
        //// TLS
        //// templates/tls-certs.yaml
        ////
        //// The chart is using cert-manager for provisioning TLS certs for
        //// brokers and proxies.
        ///
        public static ResourcesRequests ResourcesRequests {get;set;}
        public static Authentication Authentication { get; set; }

        public static ExtraConfigs ExtraConfigs { get; set; }

        public static ConfigMaps ConfigMaps { get; set; }

        public static Probes Probe { get; set; }
        public static ComponentSettings Settings { get; set; }
        public static Ports Ports { get; set; }
        public static Component Toolset { get; set; }
        public static Component AutoRecovery { get; set; } 
        public static Component ZooKeeper { get; set; }
        public static Component BookKeeper { get; set; } 
        public static Component Broker { get; set; }
        public static Component Proxy { get; set; } 
        public static Component Grafana { get; set; } 
        public static Component Prometheus { get; set; } 

        public static Component PrestoCoordinator { get; set; }
        public static Component PrestoWorker { get; set; } 
        public static Component Functions { get; set; }
        public static Component Kop { get; set; }
        public static Ingress Ingress { get; set; }

        private Component AutoRecoveryComponent()
        {
            return new Component
            {
                
                ExtraInitContainers = new List<V1Container>
                {
                    new V1Container
                        {
                            Name = "pulsar-bookkeeper-verify-clusterid",
                            Image = $"{Images.Autorecovery.Repository}:{Images.Autorecovery.Tag}",
                            ImagePullPolicy = Images.Autorecovery.PullPolicy,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{ string.Join(" ", Args.AutoRecoveryIntContainer()) },
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                         Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                    }
                                }
                            },
                            VolumeMounts = VolumeMounts.RecoveryIntContainer()
                        }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                        {
                            Name = $"{ReleaseName}-{Settings.Autorecovery.Name}",
                            Image = $"{Images.Autorecovery.Repository}:{Images.Autorecovery.Tag}",
                            ImagePullPolicy = Images.Autorecovery.PullPolicy,
                            Resources = new V1ResourceRequirements
                            {
                                Requests = new Dictionary<string, ResourceQuantity>
                                {
                                    {
                                        "memory", new ResourceQuantity(ResourcesRequests.AutoRecovery.Memory)
                                    },
                                    {
                                        "cpu", new ResourceQuantity(ResourcesRequests.AutoRecovery.Cpu)
                                    }
                                }
                            },
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{ string.Join(" ", Args.AutoRecoveryContainer()) },
                            Ports = Helpers.Ports.AutoRecovery(),
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                    }
                                }
                            },
                            VolumeMounts = VolumeMounts.RecoveryContainer()
                        }
                },
                Volumes = Volumes.Recovery()
            };
        }
        private Component ZooKeeperComponent()
        {
            return new Component
            {
                Containers = new List<V1Container>
                    {
                        new V1Container
                        {
                            Name = $"{ReleaseName}-{Settings.ZooKeeper.Name}",
                            Image = $"{Images.ZooKeeper.Repository}:{Images.ZooKeeper.Tag}",
                            ImagePullPolicy = Images.ZooKeeper.PullPolicy,
                            Resources = Settings.ZooKeeper.Resources,

                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string>{ string.Join(" ", Args.ZooKeeper()) },
                            Ports = Helpers.Ports.ZooKeeper(),
                            Env = EnvVar.ZooKeeper(),
                            EnvFrom = new List<V1EnvFromSource>
                            {
                                new V1EnvFromSource
                                {
                                    ConfigMapRef = new V1ConfigMapEnvSource
                                    {
                                        Name = $"{ReleaseName}-zookeeper"
                                    }
                                }
                            },
                            ReadinessProbe = Helpers.Probe.ExecActionReadiness(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                            LivenessProbe = Helpers.Probe.ExecActionLiviness(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                            StartupProbe = Helpers.Probe.ExecActionStartup(Probe.ZooKeeper, "bin/pulsar-zookeeper-ruok.sh"),
                            VolumeMounts = VolumeMounts.ZooKeeper()
                        }
                    },
                Volumes = Volumes.ZooKeeper(),
                PVC = VolumeClaim.ZooKeeper()
            };
        }
        private Component BrokerComponent()
        {
            return new Component
            {                
                
                ExtraInitContainers = new List<V1Container>
                {
                    // This init container will wait for zookeeper to be ready before
                    // deploying the bookies
                    new V1Container
                    {
                        Name = "wait-zookeeper-ready",
                        Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                        ImagePullPolicy = Images.Broker.PullPolicy,
                        Command = new []
                            {
                                "sh",
                                "-c"
                            },
                        Args = new List<string>{ string.Join(" ", Args.BrokerZooIntContainer()) },
                        VolumeMounts = VolumeMounts.BrokerContainer()
                    },
                    //# This init container will wait for bookkeeper to be ready before
                    //# deploying the broker
                    new V1Container
                    {
                        Name = "wait-bookkeeper-ready",
                        Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                        ImagePullPolicy = Images.Broker.PullPolicy,
                        Command = new []
                            {
                                "sh",
                                "-c"
                            },
                        Args = new List<string>{string.Join(" ", Args.BrokerBookieIntContainer()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                }
                            }
                        },
                        VolumeMounts = VolumeMounts.BrokerContainer()
                    }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.Broker.Name }",
                        Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                        ImagePullPolicy = Images.Broker.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity(ResourcesRequests.Broker.Memory)
                                },
                                {
                                    "cpu", new ResourceQuantity(ResourcesRequests.Broker.Cpu)
                                }
                            }
                        },
                        Command = new []
                        {
                            "sh",
                            "-c"
                        },
                        Args = new List<string>{string.Join(" ", Args.BrokerContainer()) },
                        Ports = Helpers.Ports.BrokerPorts(),
                        Env = EnvVar.Broker(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.Broker.Name}"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Broker, "/status.html", Ports.Broker["http"]),
                        VolumeMounts = VolumeMounts.Broker()
                    }
                },
                Volumes = Volumes.Broker()
            };
        }
        private Component BookKeeperComponent()
        {
            return new Component
            {
                ExtraInitContainers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "pulsar-bookkeeper-verify-clusterid",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Command = new[]
                        {
                             "sh",
                             "-c"
                        },
                        Args = new List<string>{string.Join(" ", Args.BookieIntContainer()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name}"
                                }
                            }
                        },
                        VolumeMounts = VolumeMounts.BookieIntContainer()
                    }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.BookKeeper.Name }",
                        Image = $"{Images.Bookie.Repository}:{Images.Bookie.Tag}",
                        ImagePullPolicy = Images.Bookie.PullPolicy,
                        Resources = new V1ResourceRequirements
                        {
                            Requests = new Dictionary<string, ResourceQuantity>
                            {
                                {
                                    "memory", new ResourceQuantity(ResourcesRequests.BookKeeper.Memory)
                                },
                                {
                                    "cpu", new ResourceQuantity(ResourcesRequests.BookKeeper.Cpu)
                                }
                            }
                        },
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.BookieContainer()) },
                        Ports = Helpers.Ports.BookKeeper(),
                        Env = EnvVar.BookKeeper(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.BookKeeper.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Bookie, "/api/v1/bookie/is_ready", Ports.Bookie["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Bookie, "/api/v1/bookie/state", Ports.Bookie["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Bookie, "/api/v1/bookie/is_ready", Ports.Bookie["http"]),
                        VolumeMounts = VolumeMounts.BookieContainer()
                    }
                },
                Volumes = Volumes.Bookie(),
                PVC = VolumeClaim.BookKeeper()
            };
        }
        private Component ProxyComponent()
        {
            return new Component
            {
                ExtraInitContainers = new List<V1Container>
                {
                    new V1Container
                        {
                            Name = "wait-zookeeper-ready",
                            Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                            ImagePullPolicy = Images.Proxy.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitZooKeeperContainer()) }
                        },
                        new V1Container
                        {
                            Name = "wait-broker-ready",
                            Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                            ImagePullPolicy = Images.Proxy.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitBrokerContainer()) } 
                        }
                },
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.Proxy.Name }",
                        Image = $"{Images.Proxy.Repository}:{Images.Proxy.Tag}",
                        ImagePullPolicy = Images.Proxy.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.ProxyContainer()) },
                        Ports = Helpers.Ports.Proxy(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.Proxy.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Proxy, "/status.html", Ports.Proxy["http"]),
                        VolumeMounts = VolumeMounts.ProxyContainer()
                    }
                },
                Volumes = Volumes.Proxy()
            };
        }
        private Component PrestoCoordinatorComponent()
        {
            return new Component
            {
                /*ExtraInitContainers = new List<V1Container> 
                {
                        new V1Container
                        {
                            Name = "wait-broker-ready",
                            Image = $"{Images.Broker.Repository}:{Images.Broker.Tag}",
                            ImagePullPolicy = Images.Broker.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitBrokerContainer()) }
                        }
                },*/
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.PrestoCoord.Name }",
                        Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                        ImagePullPolicy = Images.Presto.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args = new List<string>{ string.Join(" ", Args.PrestoCoordContainer()) },
                        Ports = Helpers.Ports.PrestoCoord(),
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.PrestoCoord.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.HttpActionReadiness(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        LivenessProbe = Helpers.Probe.HttpActionLiviness(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        StartupProbe = Helpers.Probe.HttpActionStartup(Probe.Presto, "/v1/cluster", Ports.PrestoCoordinator["http"]),
                        VolumeMounts = VolumeMounts.PrestoCoordContainer()//here
                    }
                },
                Volumes = Volumes.PrestoCoord()
            };
        }
        private Component PrestoWorkComponent()
        {
            return new Component
            {
                /*ExtraInitContainers = new List<V1Container>
                {
                        new V1Container
                        {
                            Name = "wait-presto-coord-ready",
                            Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                            ImagePullPolicy = Images.Presto.PullPolicy ,
                            Command = new []
                            {
                                "sh",
                                "-c"
                            },
                            Args = new List<string> { string.Join(" ", Args.WaitPrestoCoordContainer()) }
                        }
                },*/
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = $"{ReleaseName}-{Settings.PrestoWorker.Name }",
                        Image = $"{Images.Presto.Repository}:{Images.Presto.Tag}",
                        ImagePullPolicy = Images.Presto.PullPolicy,
                        Command = new []
                        {
                            "bash",
                            "-c"
                        },
                        Args =  new List<string>{ string.Join(" ", Args.PrestoWorker()) },
                        EnvFrom = new List<V1EnvFromSource>
                        {
                            new V1EnvFromSource
                            {
                                ConfigMapRef = new V1ConfigMapEnvSource
                                {
                                    Name = $"{ReleaseName}-{Settings.PrestoWorker.Name }"
                                }
                            }
                        },
                        ReadinessProbe = Helpers.Probe.ExecActionReadiness(Probe.PrestoWorker, "/bin/bash", "/presto/health_check.sh"),
                        LivenessProbe = Helpers.Probe.ExecActionLiviness(Probe.PrestoWorker, "/bin/bash", "/presto/health_check.sh"),
                        VolumeMounts = VolumeMounts.PrestoWorkerContainer()
                    }
                },
                Volumes = Volumes.PrestoWorker()
            };
        }
        private Component PrometheusComponent()
        {
            return new Component
            {
                Containers = Containers.Prometheus(),
                Volumes = Volumes.Prometheus(),
                PVC = VolumeClaim.Prometheus(),
                SecurityContext = new V1PodSecurityContext
                {
                    FsGroup = 65534,
                    RunAsNonRoot = true,
                    RunAsGroup = 65534,
                    RunAsUser = 65534
                }
            };
        }
        private Component GrafanaComponent()
        {
            return new Component
            {
                
            };
        }
    }
    public sealed class ProxyServiceUrl
    {
        public string BrokerHttpUrl { get; set; }
        public string BrokerHttpsUrl { get; set; }
        public string BrokerPulsarUrl { get; set; }
        public string BrokerPulsarSslUrl { get; set; }
    }
    public sealed class CertificateSecrets
    {
        public string CertificateAuthority { get; set; }
        public string AzureDnsPassword { get; set; }
        public ComponentSecret Broker { get; set; }
        public ComponentSecret Bookie { get; set; }
        public ComponentSecret Proxy { get; set; }
        public ComponentSecret Zoo { get; set; }
        public ComponentSecret Toolset { get; set; }
        public ComponentSecret Recovery { get; set; }
        public class ComponentSecret
        {
            public string Public { get; set; }
            public string Private { get; set; }
        }
    }
    public sealed class Ports
    {
        public IDictionary<string, int> Broker { get; set; } = new Dictionary<string, int>
        {
            {"http", 8080},
            {"https", 8443},
            {"pulsar", 6650},
            {"pulsarssl", 6651}
        };
        public IDictionary<string, int> Proxy { get; set; } = new Dictionary<string, int>
        {
            {"http", 8080},
            {"https", 8443},
            {"pulsar", 6650},
            {"pulsarssl", 6651}
        };
        public IDictionary<string, int> PrestoCoordinator { get; set; } = new Dictionary<string, int>
        {
            {"http", 8081},
            {"https", 4431}
        };
        public IDictionary<string, int> PrestoWorker { get; set; } = new Dictionary<string, int>
        {
            {"http", 8081},
            {"https", 4431}
        };
        public IDictionary<string, int> Bookie { get; set; } = new Dictionary<string, int>
        {
            {"http", 8000},
            {"bookie", 3181}
        };
        public IDictionary<string, int> AutoRecovery { get; set; } = new Dictionary<string, int>
        {
            {"http", 8000}
        };
        public IDictionary<string, int> Grafana { get; set; } = new Dictionary<string, int>
        {
            {"http", 3000},
            {"targetPort", 3000 }
        };
        public IDictionary<string, int> AlertManager { get; set; } = new Dictionary<string, int>
        {
            {"http", 9093}
        };
        public IDictionary<string, int> Prometheus { get; set; } = new Dictionary<string, int>
        {
            {"http", 9090}
        };
        public IDictionary<string, int> PulsarDetector { get; set; } = new Dictionary<string, int>
        {
            {"http", 9000}
        };
        public IDictionary<string, int> ZooKeeper { get; set; } = new Dictionary<string, int>
        {
            {"metrics", 8000},
            {"client", 2181},
            {"client-tls", 2281},
            {"follower", 2888},
            {"leader-election", 3888}
        };
    }
    public  sealed class Monitoring
    {
        // monitoring - prometheus
        public bool Prometheus { get; set; } = true;
        // monitoring - grafana
        public bool Grafana { get; set; } = true;
        // alerting - alert-manager
        public bool AlertManager { get; set; } = false;
        public string AlertManagerPath { get; set; }
    }
    public  sealed class Images 
    {
        public Image ZooKeeper { get; set; } = new Image();
        public Image Bookie { get; set; } = new Image();
        public Image Presto { get; set; } = new Image();
        public Image Autorecovery { get; set; } = new Image();
        public Image Broker { get; set; } = new Image();
        public Image PulsarMetadata { get; set; } = new Image();
        public Image Proxy { get; set; } = new Image();
        public Image PulsarDetector { get; set; } = new Image();
        public Image Functions { get; set; } = new Image();
        public Image Prometheus { get; set; } = new Image 
        { 
            Repository = "prom/prometheus",
            Tag = "v2.17.2"
        };
        public Image AlertManager { get; set; } = new Image
        {
            Repository = "prom/alertmanager",
            Tag = "v0.20.0"
        };
        public Image Grafana { get; set; } = new Image
        {
            Repository = "streamnative/apache-pulsar-grafana-dashboard-k8s",
            Tag = "0.0.8"
        };
        public Image PulsarManager { get; set; } = new Image
        {
            Repository = "streamnative/pulsar-manager",
            Tag = "0.3.0"
        };
        public Image NodeExporter { get; set; } = new Image
        {
            Repository = "prom/node-exporter",
            Tag = "0.16.0"
        };
        public Image Ingress { get; set; } = new Image
        {
            Repository = "us.gcr.io/k8s-artifacts-prod/ingress-nginx/controller",
            Tag = "v0.34.1"
        };
        public sealed class Image
        {
            public string ContainerName { get; set; }
            public string Repository { get; set; } = "apachepulsar/pulsar-all";
            public string Tag { get; set; } = "2.6.1";
            public string PullPolicy { get; set; } = "IfNotPresent";
            public bool HasCommand { get; set; } = false;
        }
    }
    
    public sealed class ComponentSettings
    {
        public ComponentSetting Broker { get; set; }
        public ComponentSetting ZooKeeper { get; set; }
        public ComponentSetting BookKeeper { get; set; }
        public ComponentSetting Autorecovery { get; set; }
        public ComponentSetting Proxy { get; set; }
        public ComponentSetting PrestoCoord { get; set; }
        public ComponentSetting PrestoWorker { get; set; }
        public ComponentSetting Function { get; set; }
        public ComponentSetting Toolset { get; set; }
        public ComponentSetting Kop { get; set; }
        public ComponentSetting AlertManager { get; set; }
        public ComponentSetting PulsarDetector { get; set; }
        public ComponentSetting Prometheus { get; set; }
        public ComponentSetting Grafana { get; set; }
        public ComponentSetting PulsarManager { get; set; }
    }
    public sealed class ComponentSetting
    {
        public bool Enabled { get; set; }
        public bool AntiAffinity { get; set; } = true;
        public int Replicas { get; set; }
        public string Name { get; set; }
        public string Service { get; set; }
        public string Host { get; set; }
        public Offload Offload { get; set; } = new Offload();
        public V1ResourceRequirements Resources { get; set; } = new V1ResourceRequirements();
        public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>();
        public List<V1Toleration> Tolerations { get; set; } = new List<V1Toleration>();
        public ProxyServiceUrl ProxyServiceUrl { get; set; } = new ProxyServiceUrl();
        public bool UsePolicyPodDisruptionBudget { get; set; }
        public bool EnableFunctionCustomizerRuntime { get; set; } = false;
        public string PulsarFunctionsExtraClasspath { get; set; }
        public string RuntimeCustomizerClassName { get; set; }
        public string PodManagementPolicy { get; set; }
        public string UpdateStrategy { get; set; }
        public int GracePeriodSeconds { get; set; }
        public bool Persistence { get; set; } = true;
        public bool LocalStorage { get; set; } = false;
        public string ZooConnect { get; set; }
        public string ZNode { get; set; }
        public Storage Storage { get; set; }
        public Annotations Annotations { get; set; } = new Annotations();
        public IDictionary<string, string> ExtraConfigMap { get; set; } = new Dictionary<string, string>();
    }
    public class ConfigmapReloads
    {
        public ConfigmapReload Prometheus { get; set; } = new ConfigmapReload
        {
            Name = "configmap-reload",
            Enabled = true,
            Image = new Images.Image
            {
                Repository = "jimmidyson/configmap-reload",
                Tag = "v0.3.0",
                PullPolicy = "IfNotPresent"
            }
        };
        public ConfigmapReload AlertManager { get; set; } = new ConfigmapReload();
        public sealed class ConfigmapReload
        {
            public bool Enabled { get; set; }
            public string Name { get; set; }
            public ResourcesRequest ResourcesRequest { get; set; } = new ResourcesRequest();
            public List<VolumeMount> ExtraConfigmapMounts { get; set; } = new List<VolumeMount>();
            public List<string> ExtraVolumeDirs { get; set; } = new List<string>();
            public Dictionary<string, string> ExtraArgs { get; set; } = new Dictionary<string, string>();
            public Images.Image Image { get; set; } = new Images.Image();
        }
        public class VolumeMount
        {
            public string Name { get; set; }
            public string MountPath {get; set;}
            public bool Readonly { get; set; }
            public string SubPath { get; set; }
            public V1ConfigMapVolumeSource ConfigMap { get; set; }
        }
    }
    public class Annotations
    {
        public IDictionary<string, string> Service { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Template { get; set; } = new Dictionary<string, string>();
    }
    public sealed class Ingress 
    {
        public bool Rbac { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public int Replicas { get; set; } = 1;
        public int GracePeriodSeconds { get; set; } = 30;

        public IngressSetting Proxy { get; set; } = new IngressSetting();
        public IngressSetting Presto { get; set; } = new IngressSetting();
        public IngressSetting Broker { get; set; } = new IngressSetting();
        public IngressSetting Grafana { get; set; } = new IngressSetting();
        public List<HttpRule> HttpRules { get; set; } = new List<HttpRule> 
        { 
            new HttpRule
            {
                Host = "grafana.splsar.ga",
                Port = 3000,
                Path = "/",
                Tls = true,
                ServiceName = $"{Values.ReleaseName}-{Values.Settings.Grafana.Name}"
            }, 
            new HttpRule
            {
                Host = "presto.splsar.ga",
                Port = 8081,
                Path = "/",
                Tls = true,
                ServiceName = Values.Settings.PrestoCoord.Service
            }
        };
        public sealed class IngressSetting
        {
            public bool Enabled { get; set; } = true;
            public bool Tls { get; set; } = true;
            public string Type { get; set; }
            public IDictionary<string,string> Annotations { get; set; }
            public IDictionary<string,string> ExtraSpec { get; set; }
        }
        public sealed class HttpRule
        {
            public bool Tls { get; set; } = true;
            public string Host { get; set; }
            public int Port { get; set; }
            public string Path { get; set; }
            public string ServiceName { get; set; }
        }
    }
    public sealed class Tls
    {
        public bool Enabled { get; set; } = false;
        
        //Should disable - not
        public ComponentTls ZooKeeper { get; set; } = new ComponentTls
        {
            Enabled = true,
            CertName = "tls-zookeeper"
        };
        public ComponentTls Proxy { get; set; } = new ComponentTls
        {
            Enabled = true,
            CertName = "tls-proxy"
        };
        public ComponentTls Broker { get; set; } = new ComponentTls 
        { 
            Enabled = true,
            CertName = "tls-broker"
        };
        public ComponentTls Bookie { get; set; } = new ComponentTls 
        { 
            Enabled = true,
            CertName = "tls-bookie"
        };
        public ComponentTls AutoRecovery { get; set; } = new ComponentTls 
        { 
            Enabled = false,
            CertName = "tls-recovery"
        };
        public ComponentTls ToolSet { get; set; } = new ComponentTls 
        { 
            Enabled = false, 
            CertName = "tls-toolset"
        };
        public class ComponentTls 
        {
            public bool Enabled { get; set; } = false;
            public string CertName { get; set; }
        }
    }

    public sealed class Authentication
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; } = "jwt";
        // Enable JWT authentication
        // If the token is generated by a secret key, set the usingSecretKey as true.
        // If the token is generated by a private key, set the usingSecretKey as false.
        public bool UsingJwtSecretKey { get; set; } = false;
        public bool Authorization { get; set; } = false;
        public bool Vault { get; set; } = false;
        public SuperUsers Users { get; set; } = new SuperUsers();
        public sealed class SuperUsers
        {
            // broker to broker communication
            public string Broker { get; set; } = "broker-admin";
            // proxy to broker communication
            public string Proxy { get; set; } = "proxy-admin";
            // pulsar-admin client to broker/proxy communication
            public string Client { get; set; } = "admin";
            //pulsar-manager to broker/proxy communication
            public string PulsarManager { get; set; } = "pulsar-manager-admin";
        }
    }
    public sealed class ExtraConfigs
    {
        public ExtraConfig ZooKeeper { get; set; }
        public ExtraConfig Proxy { get; set; }
        public ExtraConfig Broker { get; set; }
        public ExtraConfig Bookie { get; set; }
        public ExtraConfig PrestoCoordinator { get; set; }
        public ExtraConfig PrestoWorker { get; set; }
        public ExtraConfig AutoRecovery { get; set; }
        public ExtraConfig Prometheus { get; set; }
        public ExtraConfig Grafana { get; set; }
    }
    public sealed class Probes
    {
        public ComponentProbe Broker { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 60,
                PeriodSeconds = 10
            }
        };
        
        public ComponentProbe ZooKeeper { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };

        public ComponentProbe Presto { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };
        public ComponentProbe PrestoWorker { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };
        public ComponentProbe Proxy { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };
        public ComponentProbe Prometheus { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 10,
                InitialDelaySeconds = 30,
                PeriodSeconds = 10
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            }
        };
        public ComponentProbe Bookie { get; set; } = new ComponentProbe
        {
            Liveness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 60,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Readiness = new ProbeOptions
            {
                Enabled = true,
                FailureThreshold = 60,
                InitialDelaySeconds = 10,
                PeriodSeconds = 30
            },
            Startup = new ProbeOptions
            {
                Enabled = false,
                FailureThreshold = 30,
                InitialDelaySeconds = 60,
                PeriodSeconds = 30
            }
        };
        public sealed class ComponentProbe
        {
            public ProbeOptions Liveness { get; set; }
            public ProbeOptions Readiness { get; set; }
            public ProbeOptions Startup { get; set; }
        }
        public sealed class ProbeOptions
        {
            public bool Enabled { get; set; } = false;
            public int FailureThreshold { get; set; }
            public int InitialDelaySeconds { get; set; }
            public int PeriodSeconds { get; set; }
        }
    }
    public class Component
    {
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1PersistentVolumeClaim> PVC { get; set; } = new List<V1PersistentVolumeClaim>();
        public List<V1Volume> Volumes { get; set; } = new List<V1Volume>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public List<V1Toleration> Tolerations { get; set; } = new List<V1Toleration>();
        public V1PodSecurityContext SecurityContext { get; set; } = new V1PodSecurityContext { };
        public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>();
    }
    public sealed class ResourcesRequests
    {
        public ResourcesRequest AutoRecovery { get; set; } = new ResourcesRequest { Memory = "64Mi", Cpu = "0.05" };
        public ResourcesRequest ZooKeeper { get; set; } = new ResourcesRequest { Memory = "256Mi", Cpu = "0.1" };
        public ResourcesRequest BookKeeper { get; set; } = new ResourcesRequest { Memory = "512Mi", Cpu = "0.2" };
        public ResourcesRequest Broker { get; set; } = new ResourcesRequest { Memory = "512Mi", Cpu = "0.2" };
        public ResourcesRequest Grafana { get; set; } = new ResourcesRequest { Memory = "250Mi", Cpu = "0.1" };
        public ResourcesRequest Proxy { get; set; }
        public ResourcesRequest PrestoCoordinator { get; set; }
        public ResourcesRequest PrestoWorker { get; set; }

    }

    public sealed class ResourcesRequest
    {
        public string Memory { get; set; }
        public string Cpu { get; set; }

    }
    public sealed class Offload
    {
        public bool Enabled { get; set; }
        public string ManagedLedgerOffloadDriver { get; set; }
        public OffloadSetting Gcs { get; set; } = new OffloadSetting();
        public OffloadSetting Azure { get; set; } = new OffloadSetting();
        public OffloadSetting S3 { get; set; } = new OffloadSetting();
        public sealed class OffloadSetting
        {
            public bool Enabled { get; set; }
            public string Region { get; set; }
            public string Bucket { get; set; }
            public long MaxBlockSizeInBytes { get; set; }
            public long ReadBufferSizeInBytes { get; set; }
        }
    }
    public sealed class Storage
    {
        public string ClassName { get; set; } = "default";//Each AKS cluster includes four pre-created storage classes(default,azurefile,azurefile-premium,managed-premium)
        public string Provisioner { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string Size { get; set; }
        public string JournalSize { get; set; }
        public string LedgerSize { get; set; }
    }
    public class ExtraConfig
    {
        public List<V1Container> ExtraInitContainers { get; set; } = new List<V1Container>();
        public List<V1Container> Containers { get; set; } = new List<V1Container>();
        public IDictionary<string, object> Holder { get; set; } = new Dictionary<string, object>();
    }
    public sealed class ConfigMaps
    {
        public IDictionary<string, string> ZooKeeper { get; set; } = Config.ZooKeeper().RemoveRN();
        public IDictionary<string, string> BookKeeper { get; set; } = Config.BookKeeper().RemoveRN();
        public IDictionary<string, string> Broker { get; set; } = Config.Broker().RemoveRN();
        public IDictionary<string, string> PrestoCoordinator { get; set; } = Config.PrestoCoord(Values.Settings.PrestoCoord.Replicas > 0 ? "false" : "true").RemoveRN();
        public IDictionary<string, string> PrestoWorker { get; set; } = Config.PrestoWorker().RemoveRN();
        public IDictionary<string, string> Proxy { get; set; } = Config.Proxy().RemoveRN();
        public IDictionary<string, string> AutoRecovery { get; set; } = new Dictionary<string, string> { { "BOOKIE_MEM", "-Xms64m -Xmx64m" } };
        public IDictionary<string, string> Functions { get; set; }
        public IDictionary<string, string> Toolset { get; set; } = Config.ToolSet().RemoveRN();
        public IDictionary<string, string> Prometheus { get; set; } = Config.Prometheus();
        public IDictionary<string, string> PulsarManager { get; set; }
        public IDictionary<string, string> PulsarDetector { get; set; }
        public IDictionary<string, string> Grafana { get; set; }
        public IDictionary<string, string> AlertManager { get; set; }
    }
    public class Rbac
    {
        public bool Enabled { get; set; } = true;
        public string RoleName { get; set; } = "pulsar-operator";
        public string RoleNameBinding { get; set; } = "pulsar-operator-cluster-role-binding";
    }
}

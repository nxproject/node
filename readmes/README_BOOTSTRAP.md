# Bootstrap

This script is run in the host after Docker is installed:

```
ufw allow from 172.17.0.0/16
ufw allow 80
ufw allow 443

mkdir -p /etc/nxproject
echo '{ "iptables": false }' > /etc/docker/daemon.json
sed -i -e 's/DEFAULT_FORWARD_POLICY="DROP"/DEFAULT_FORWARD_POLICY="ACCEPT"/g' /etc/default/ufw
ufw reload
iptables -t nat -A POSTROUTING ! -o docker0 -s 172.17.0.0/16 -j MASQUERADE
systemctl stop docker
rm /etc/systemd/system/docker-tcp.socket
echo '[Unit]' > /etc/systemd/system/docker-tcp.socket
echo 'Description=Docker TCP API Socket' >> /etc/systemd/system/docker-tcp.socket
echo '[Socket]' >> /etc/systemd/system/docker-tcp.socket
echo 'ListenStream=2375' >> /etc/systemd/system/docker-tcp.socket
echo 'BindIPv6Only=both' >> /etc/systemd/system/docker-tcp.socket
echo 'Service=docker.service' >> /etc/systemd/system/docker-tcp.socket
echo '[Install]' >> /etc/systemd/system/docker-tcp.socket
echo 'WantedBy=sockets.target' >> /etc/systemd/system/docker-tcp.socket
systemctl enable docker-tcp.socket
systemctl enable docker.socket
systemctl start docker-tcp.socket
systemctl start docker
iptables-save | sed '/^#/d' | sed 's/\[[^][]*:[^][]*\]/\[0:0\]/g' > /etc/nxproject/iptable.bkp

docker pull nxproject/processor:site
echo "" > .bash_history

docker run -w /etc/wd nxproject/site:processor dotnet NXNode.dll
```


[Home](../README.md)
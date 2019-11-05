FROM centos:latest
COPY mongodb-org-4.2.repo /etc/yum.repos.d/mongodb-org-4.2.repo
RUN yum install -y mongodb-org-shell
RUN yum install -y cronie
COPY scripts /root/scripts
COPY startup.sh /usr/local/bin
COPY update-cronjob /root
ENTRYPOINT ["startup.sh"]
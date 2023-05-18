docker run --name proxy -d -p 80:80 -p 443:443 \
            -v "/etc/letsencrypt:/etc/letsencrypt" \
            -v "/root/proxy/assistant.dmitrybodnar.com/conf:/etc/nginx/conf.d" \
            -v "/root/proxy/assistant.dmitrybodnar.com/www:/usr/share/nginx/html:ro" \
             nginx

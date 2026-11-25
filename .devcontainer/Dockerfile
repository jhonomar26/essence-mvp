FROM mcr.microsoft.com/dotnet/sdk:10.0

# Node 22 + pnpm, copied from the official image to avoid distro-old Node
COPY --from=node:22-slim /usr/local/ /usr/local/
RUN npm install -g pnpm

RUN apt-get update \
 && apt-get install -y --no-install-recommends git gh sudo postgresql-client curl ca-certificates \
 && rm -rf /var/lib/apt/lists/*

# uid 1000 matches the host user; with userns keep-id, files stay owned by you
RUN if id ubuntu >/dev/null 2>&1; then \
      groupmod -n dev ubuntu && usermod -l dev -d /home/dev -m -s /bin/bash ubuntu; \
    else useradd -m -u 1000 -s /bin/bash dev; fi \
 && echo 'dev ALL=(ALL) NOPASSWD:ALL' > /etc/sudoers.d/dev \
 && chmod 440 /etc/sudoers.d/dev

USER dev
RUN curl -fsSL https://claude.ai/install.sh | bash
ENV PATH="/home/dev/.local/bin:${PATH}"
WORKDIR /workspace

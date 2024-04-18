# 👑 DagoniteEmpire
![image](https://github.com/KrystianKempski/DagoniteEmpire/assets/19647553/36e5a213-fd3d-4849-bae7-826be70e2f0f)

## ⚔️ About
A comprehensive tool for starting an online, text-based RPG campaign, enabling multiple players to start long-term games in an original, well-defined world. Each player creates their own character with special abilities and skill sets, who then become a part of an adventure team with other players in a campaign led by a Game Master. 
Application database is made on PostGreSQL.

## 🎲 Try it yourself
You can access development version of deployed application here:
[dagonite-empire.drik.it](https://dagonite-empire.drik.it)

You can use default test users or create your own account.

Default player account:
  - Login: `player@example.com`
  - Password: `Guest123*`

Default Game Master account:
  - Login: `gm@example.com`
  - Password: `Guest123*`

🚧 Work in progress.

## 🏗️ Deploy
Application is built automatically when new release is published using Github Actions as a Docker image and pushed to DockerHub. FluxCD residing on private Kubernetes cluster is monitoring DockerHub registry and when a new version is ready it recreates the app container with new image wihitn 5 minutes. Deployment configuration is set using custom helm chart available in [drik-homelab-helm-charts](https://github.com/drikqlis/drik-homelab-helm-charts) repository and managed by FluxCD.
 

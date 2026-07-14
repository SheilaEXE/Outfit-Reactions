# Outfit Reactions

Um mod para **Stardew Valley** que faz NPCs comentarem sobre as roupas do seu personagem de forma dinâmica sempre que você troca de visual, com exclusividade pelo [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969).

Feito por **NatrollEXE**.

> **O Outfit Reactions não substitui nem remove diálogos do jogo base ou de outros mods.** Ele apenas adiciona uma fala extra de reação ao visual do jogador quando as condições do mod são atendidas; os diálogos que o NPC já teria continuam disponíveis normalmente.

---

## ✨ Funcionalidades

* **Reações via IA**: cônjuges e NPCs próximos reagem ao seu outfit atual usando IA, gerando falas únicas a cada troca de roupa.
* **Suporte a chapéus e calças vanilla**: reage tanto a itens do Fashion Sense quanto a alguns chapéus vanilla comuns do jogo.
* **Itens especiais com segredo**: sistema de itens especiais (`assets/special-reactions`) que permite criar reações específicas para peças marcantes (Atualmente tem apenas do Short Roxo da Sorte).
* **Memória de outfit**: NPCs lembram outfits e itens especiais que já viram antes, reagindo com familiaridade em vez de repetir a mesma reação de "primeira vez".
* **Compatibilidade com clima e localização**: as reações levam em conta se o NPC está em ambiente interno ou externo, se está sol ou se está chovendo, horário do dia e datas festivas.
* **Content packs próprios** podem expandir/customizar características de NPCs (`assets/npc-characteristics`) sem exigir Content Patcher.
* **Configurável via Generic Mod Config Menu** (opcional) — vários modos de reação (combinado ou focado no item especial), múltiplos perfis de IA, etc.

## 📋 Requisitos

* [SMAPI](https://smapi.io/) 4.0.0+
* [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969) 7.5.0+ (obrigatório)
* [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (opcional, recomendado para configurar facilmente)
* Uma chave de API de algum provedor de IA compatível (ex: [OpenRouter](https://openrouter.ai/), Google Gemini) para gerar as reações.

## 🔧 Instalação

1. Instale o [SMAPI](https://smapi.io/).
2. Baixe e instale o Fashion Sense.
3. Baixe a última release do Outfit Reactions e extraia a pasta na sua pasta `Mods`.
4. Configure sua chave de API de IA pelo Generic Mod Config Menu (ou editando o `config.json` gerado após a primeira execução).

## ⚙️ Configuração

O mod pode ser configurado através do Generic Mod Config Menu, incluindo:

* Perfis de IA (provedor, modelo, chave de API, endpoint customizado).
* Modo de reação a chapéus/itens especiais vanilla (combinado com o outfit ou focado só no item).
* Ativação de análise visual (vision) para os modelos que suportam.
* Diversos outros ajustes de comportamento e frequência de reações.

## 🧩 Criando conteúdo customizado (Content Packs próprios)

Este mod é extensível por content packs próprios:

* `assets/npc-characteristics/*.json` — defina a personalidade/estilo de fala de NPCs para moldar como eles reagem.
* `assets/special-reactions/*.json` — defina itens especiais (roupas, chapéus, calças) com reações e segredos customizados.
* `assets/prompts/prompts.json` — ajuste as regras/instruções enviadas para o modelo de IA.

## 📜 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE) — sinta-se livre para usar, modificar e distribuir, mantendo os créditos.

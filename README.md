# OpenAI for Grasshopper

This Grasshopper plugin (.gha) is an integration of the OpenAI API for .NET: https://github.com/betalgo/openai that I made quickly to play with the OpenAI API from Grasshopper (Rhino 8).

Keep in mind that it is still a work in progress, not everything has been tested, and it may not be continued. Also note that OpenAI has a paid API, so it's not free to use.

<img style="center" src=".\Resources\Canvas at 13;49;43.png"></img>		  

### Features
- [x] ChatGPT
- [x] Image (DALL·E)
- [x] Models
- [x] Completions
- [x] Edit (uncomplete)
- [x] Embeddings
- [x] Files (untested)
- [x] Fine-tunes (untested)
- [x] Codex
- [ ] Whisper
- [ ] Moderation
- [ ] Rate limit support

### Set up
1. Download lastest [release](https://github.com/DanielAbalde/OpenAI-for-Grasshopper/releases).
2. Unzip and move 'Open AI for Grasshopper' folder to your Grasshopper libraries folder.
3. Download [Sample.gh](https://github.com/DanielAbalde/OpenAI-for-Grasshopper/releases) file and open it. There are errors on components because it needs your API key.
4. Log-in or create an OpenAI account from [OpenAI site](https://beta.openai.com/).
5. Create a new secret key from your [OpenAI account](https://beta.openai.com/account/api-keys).
6. Copy this API key and paste it in the panel of Sample.gh.
7. Be sure to check your usage frequently from your [OpenAI account](https://beta.openai.com/account/usage).	Please note that you may need to put your bank card and be charged for using it. Make sure you understand pricing and usage limits.
8. Enjoy!


### Collaborate
- Issues and pull request are welcome.
- Contact me via Discord: DaniGA#9856.
from discord import Intents, Client, Message, app_commands, Poll
from os import getenv
from discord.ext import tasks
from datetime import timedelta
from quiz import Quiz
from word_manager import WordManager

"""
TODO: make the channel_to_send_to environment variable
something which can be changed via a slash command
"""

BOT_AUTHORIZATION_CODE = getenv("DISCORD_API_TOKEN")
APPLICATION_ID = getenv("APPLICATION_ID")
channelToSendTo = getenv("CHANNEL_TO_SEND_TO")

# bot setup
intents: Intents = Intents.default()
intents.message_content = True

client: Client = Client(intents=intents, application_id=APPLICATION_ID)
tree = app_commands.CommandTree(client)

# Stuff I need to access in my dictionary_loop() coroutine; idk how else to do this.
# TODO learn a better way to do this
global CHANNEL_TO_SEND_TO
global quiz
global word_manager
quiz: Quiz = None
CHANNEL_TO_SEND_TO = int(channelToSendTo)
word_manager: WordManager = WordManager()

async def sync_bot_commands() -> None:
    try:
        synced = await tree.sync(guild=None)
        print(f"Synced {len(synced)} commands(s)")
    except Exception as e:
        print(e)

@client.event
async def on_ready() -> None:
    print(f"{client.user} is now running!")
    # https://community.latenode.com/t/discord-bot-encounters-runtimeerror-no-running-event-loop-while-initiating-asyncio-coroutine-task/24700/2
    # That comment says to have this: "The is_running() check prevents the task from starting multiple times if the bot reconnects"
    if not dictionary_loop.is_running():
        dictionary_loop.start()
    await sync_bot_commands()
    global quiz
    global CHANNEL_TO_SEND_TO
    quiz = Quiz(discord_channel_to_send_poll=client.get_channel(CHANNEL_TO_SEND_TO))

@client.event
async def on_message(message: Message) -> None:
    if not message.content:
        # print("(Message was empty because intents were likely not enabled!)")
        return
    
    if message.author.bot:  # Check if the message is from a bot to avoid infinite loops
        return

@tasks.loop(hours=24)
async def dictionary_loop():
    word, dictionary_entry_of_word = word_manager.get_word_and_printable_definition()
    definition_start_index = dictionary_entry_of_word.find("(") # the index where the (verb) or whatever starts
    definition = dictionary_entry_of_word[definition_start_index:]
    print(f"dictionary_loop executed again; the word is: {dictionary_entry_of_word}\n")
    global CHANNEL_TO_SEND_TO
    channel = client.get_channel(CHANNEL_TO_SEND_TO)
    
    # https://discordpy.readthedocs.io/en/stable/api.html?highlight=poll#discord.Poll
    poll = Poll(question="Did you know this word?", duration=timedelta(hours=23), multiple=True)
    poll.add_answer(text="Yes")
    poll.add_answer(text="No")
    poll.add_answer(text="Idk")
    word_with_spoilered_definition = f"**{word}**||{definition}||"
    await channel.send(content=f"The word of the day is: {word_with_spoilered_definition}\n")
    await channel.send(poll=poll)

    global quiz
    quiz.add(word, dictionary_entry_of_word)

# main entry point to run the code, running the bot. This needs to be below all the events in order for the
# bot to work with the events registered to it.
def main() -> None:
    print("main called")
    # Put all startup code in on_ready() function, not here!
    client.run(token=BOT_AUTHORIZATION_CODE)

if __name__ == '__main__':
    main()  # calls main function which runs the bot

from discord import Intents, Client, Message, app_commands, Poll
from wordfreq import word_frequency
from requests import get
from os import getenv
from random import randrange
from asyncio import sleep
from discord.ext import commands, tasks
from datetime import datetime, timedelta

"""
TODO: 
Some potential requirements:
random word, make a poll "do you know this word"?
after a month of doing this, there is a quiz
So every 30 sends, it will take a random definition which is NOT part of the past 3... 
and then it will have 9 words or an IDK option
OR, it will have a random word which is NOT part of the past 3...
and then it will have 9 definitions or an IDK option
"""

class QuizData:
    def __init__(self, count_num, word, definition):
        self.count_num = count_num
        self.word = word
        self.definition = definition

class Quiz:
    def __init__(self, channel):
        self.data: list[QuizData] = []
        self.channel = channel
        self.count = 0
    
    async def add(self, word, definition):
        self.count += 1
        quizData: QuizData = QuizData(self.count, word, definition)
        self.data.append(quizData)
        print(f"add called. Count is: {self.count} with data of: {self.data}")
        if self.count == 2:
            while (True):
                random_num = randrange(3) # its exclusive for upper bound
                thing = filter(x.count_num == random_num for x in self.data) # TODO this line throws idk why
                print(thing)
                random_definitions: set[str, str] = {}
                random_definitions.add()
                # TODO make 2 polls based on above TODO multiline comment
                # poll = Poll(question="Did you know this word?", duration=timedelta(hours=1), multiple=True)
                # poll.add_answer(text="Yes")
                # poll.add_answer(text="No")
                # poll.add_answer(text="Idk")
    
                await self.channel.send(poll=poll)

BOT_AUTHORIZATION_CODE = getenv("DISCORD_API_TOKEN")
APPLICATION_ID = getenv("APPLICATION_ID")
channelToSendTo = getenv("CHANNEL_TO_SEND_TO")

# bot setup
intents: Intents = Intents.default()
intents.message_content = True

client: Client = Client(intents=intents, application_id=APPLICATION_ID)
tree = app_commands.CommandTree(client)

# Stuff I need to access in my dictionary_loop() coroutine; idk how else to do this.
global word_list
global CHANNEL_TO_SEND_TO
global quiz
CHANNEL_TO_SEND_TO = channelToSendTo
word_list: list[str] = []
quiz: Quiz = Quiz(client.get_channel(CHANNEL_TO_SEND_TO))

async def sync_bot_commands() -> None:
    try:
        synced = await tree.sync(guild=None)
        print(f"Synced {len(synced)} commands(s)")
    except Exception as e:
        print(e)

@client.event
async def on_ready() -> None:  # on ready, returns nothing
    print(f"{client.user} is now running!")
    # https://community.latenode.com/t/discord-bot-encounters-runtimeerror-no-running-event-loop-while-initiating-asyncio-coroutine-task/24700/2
    # That comment says to have this: "The is_running() check prevents the task from starting multiple times if the bot reconnects"
    if not dictionary_loop.is_running():
        dictionary_loop.start()
    await sync_bot_commands()

@client.event
async def on_message(message: Message) -> None:
    if not message.content:
        print("(Message was empty because intents were likely not enabled!)")
        return
    
    if message.author.bot:  # Check if the message is from a bot to avoid infinite loops
        return

# Gets the definition of the word, returning the API response status code and a definition string.
# The string can be null iff there is a bad response or exception. The status code is -1 on exception.
def get_definition(word: str) -> tuple[int, str]:
    try:
        url = f"https://api.dictionaryapi.dev/api/v2/entries/en/{word}"
        response = get(url)
        response_string = None
    
        if response.status_code == 200:
            data = response.json()
            # I just get the first definition here. TODO is this the best move?
            meaning = data[0]['meanings'][0]
            definition = meaning['definitions'][0]['definition']
            part_of_speech = meaning['partOfSpeech']
            response_string = f"**{word}** (*{part_of_speech}*): {definition}"
        return response.status_code, response_string
    except Exception as e:
        print(f"get_definition exception1: {str(e)}")
        return -1, None

def word_is_valid(word: str) -> bool:
    freq = word_frequency(word, 'en')
    if freq > 1e-6:
        return False
    # The list of words I'm using sometimes contains words with spaces; I don't want words with spaces.
    if " " in word:
        return False
    word_ends_with_ing = len(word) > 3 and word[-3:] == "ing"
    if word_ends_with_ing:
        return False
    word_ends_with_ed = len(word) > 2 and word[-2:] == "ed"
    if word_ends_with_ed:
        return False
    return True

def populate_word_list(word_list_file_name_with_extension: str, encoding: str) -> None:
    # For some reason doing word_list = [] doesn't work... seems to create a new local variable
    # rather than using the global word_list.
    word_list.clear()  
    with open(word_list_file_name_with_extension, encoding=encoding) as infile:
        for line in infile:
            if word_is_valid(line.strip()):
                word_list.append(line)

def get_word_and_printable_definition() -> tuple[str, str]:
    while True:
        # So I dont forget: len() is constant time for lists - the length is stored as a field.
        # Also in terms of inclusive/exclusive: randrange(10) can generate 0-9.
        random_num = randrange(len(word_list))
        print(f"The random number is {random_num}")
        word = word_list[random_num]
        definition_response: tuple[int, str] = get_definition(word)
        # bad solution to the problem of some definitions maybe not being there, which can happen even though the
        # list of words is directly from the API's repo.
        if definition_response[0] == 404:
            print(f"Definition is 404, word: {word}")
            continue
        elif definition_response[0] != 200:
            print(f"Some disasterous error occurred, idk why. Error code: {definition_response[0]}")
            # Assuming that if the API is failing or something, I don't want to retry at the present time. I think
            # just showing a word but no definition is perfectly OK, to be honest.
            return word, f"An error occurred - could not fetch definition for some reason (error code {definition_response[0]})"
        else:
            return word, definition_response[1]

@tasks.loop(seconds=10)
async def dictionary_loop():
    print("loop called")
    # This file was found from the following repo on 2/16/2026:
    # https://github.com/meetDeveloper/freeDictionaryAPI/blob/master/meta/wordList/english.txt
    if len(word_list) == 0:
        # I expect this to only get ran once when the bot first starts up.
        print("Generating word list")
        populate_word_list("src/english.txt", "utf-8")
        print(f"Done populating word list2: It has a length of: {len(word_list)}")
    word_and_printable_defintion: tuple[str,str] = get_word_and_printable_definition()
    word = word_and_printable_defintion[0]
    print(f"attempting to remove word: {word} from the list of length: {len(word_list)}.\n")
    word_list.remove(word)
    print(f"Removed word from the list, now its length: {len(word_list)}\n")
    dictionary_entry_of_word = word_and_printable_defintion[1]
    print(f"The word is: {dictionary_entry_of_word}\n")    
    channel = client.get_channel(CHANNEL_TO_SEND_TO) # TODO this doesnt work yet
    # https://discordpy.readthedocs.io/en/stable/api.html?highlight=poll#discord.Poll
    poll = Poll(question="Did you know this word?", duration=timedelta(hours=20), multiple=True)
    poll.add_answer(text="Yes")
    poll.add_answer(text="No")
    poll.add_answer(text="Idk")
    
    await channel.send(content=f"The word is: {dictionary_entry_of_word}\n")
    await channel.send(poll=poll)

    await quiz.add(word, dictionary_entry_of_word)

    print("all done")


# main entry point to run the code, running the bot. This needs to be below all the events in order for the
# bot to work with the events registered to it.
def main() -> None:
    print("main called")
    # Put all startup code in on_ready() function, not here!
    client.run(token=BOT_AUTHORIZATION_CODE)

if __name__ == '__main__':
    main()  # calls main function which runs the bot

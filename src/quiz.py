from discord import Poll
from asyncio import sleep, create_task
from random import choice, sample
from datetime import timedelta

"""
TODO look into some mechanism to send proper quizzes without the constraints of discord's polling feature... I know
there are discord quiz bots that exist (google search "discord quizzes") - might be good to implement something like
that if its desired, so that it would be possible to do a word and 10 definitions (constrained by poll char limits).
"""

class QuizData:
    def __init__(self, word, definition):
        self.word = word
        self.definition = definition

class Quiz:
    def __init__(self, discord_channel_to_send_poll):
        self._previously_sent_words: list[QuizData] = []
        self.discord_channel_to_send_poll = discord_channel_to_send_poll

    def add(self, word, definition) -> None:
        self._previously_sent_words.append(QuizData(word, definition))
        if len(self._previously_sent_words) == 30:
            # Not awaiting intentionally because I want to send the answer later by just awaiting asyncio.sleep
            create_task(self.__setup_poll())

    async def __setup_poll(self) -> None:
        random_words_with_definitions: list[QuizData] = self.__get_10_random_words_with_definitions()
        correct_word_and_definition: QuizData = self.__pick_question_and_answer(random_words_with_definitions)
        poll_duration = timedelta(hours=22)  # Needs to be in hours per documentation
        await self.__build_and_send_poll(random_words_with_definitions, correct_word_and_definition.definition, poll_duration)
        self._previously_sent_words.clear()
        await sleep(poll_duration.total_seconds() + 5) # Get the poll_duration time, add 5 seconds, then send the answer
        await self.discord_channel_to_send_poll.send(f"The answer was: **{correct_word_and_definition.word}**")

    def __get_10_random_words_with_definitions(self) -> list[QuizData]:
        candiates = self._previously_sent_words[:27] # don't include 3 most recent words in the poll due to recency
        return sample(candiates, 10)
    
    def __pick_question_and_answer(self, random_definitions) -> QuizData:
        correct_answer = choice(random_definitions)
        definition_start_index = correct_answer.definition.find(':')
        definition = correct_answer.definition[definition_start_index + 1:]
        correct_answer: QuizData = QuizData(correct_answer.word[:55], definition) # the definition is truncated when poll is sent
        return correct_answer

    async def __build_and_send_poll(self, chosen_words_and_definitions_to_use_in_poll, chosen_definition, poll_duration) -> None:
        poll = Poll(question=f"What word has this definition: {chosen_definition}"[:300], duration=poll_duration, multiple=True)
        for val in chosen_words_and_definitions_to_use_in_poll:
            poll.add_answer(text=val.word[:55])
        await self.discord_channel_to_send_poll.send(poll=poll)

import { ref } from "vue";

export default {
    template:/*html*/`
        <div class="p-4 bg-white dark:bg-black rounded-md">
            <h1 class="text-xl font-semibold mb-4">Sentiment Analysis Result</h1>
            <div class="text-lg">
                <p v-if="sentiment == null || sentiment.sentiment == null" class="text-gray-500">Awaiting Analysis...</p>
                <p v-else-if="sentiment.sentiment === 'Positive'" class="text-green-500">Positive Sentiment</p>
                <p v-else-if="sentiment.sentiment === 'Neutral'" class="text-yellow-500">Neutral Sentiment</p>
                <p v-else-if="sentiment.sentiment === 'Negative'" class="text-red-500">Negative Sentiment</p>
                <p>Based on text: {{sentiment.text}}</p>
            </div>
        </div>
    `,
    props: ['sentiment'],
    setup(props) {
        let sentiment = ref(props.sentiment);
        console.log(sentiment.value)
        
        return { sentiment };
    }
}

// AudioRecorderComponent.mjs
import { ref } from "vue";
import { useClient } from "@servicestack/vue";
import { AudioRecorder } from "../AudioRecorder.mjs";
import Sentiment from "./Sentiment.mjs";
import { CreateChat, CreateRecording } from "../dtos.mjs";  // Import your DTOs

export default {
    template:/*html*/`<div class="relative z-10 flex flex-col p-4 bg-white dark:bg-black rounded-md">
  <div class="mt-4">
    <input 
      v-model="transcript"
      type="text" 
      placeholder="Enter text here"
      class="p-2 w-full border rounded-md focus:ring focus:ring-indigo-200 dark:bg-gray-800 dark:text-white"
    />
  </div>
  
  <div class="flex mt-2">
    <button @click="toggleRecording" class="bg-indigo-600 hover:bg-indigo-700 text-white py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 dark:ring-offset-black">
      {{ isRecording ? 'Stop Recording' : 'Start Recording' }}
    </button>
  
    <div v-if="isLoading" class="ml-4">
      <svg class="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.372 0 0 5.372 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 1.33.26 2.59.741 3.741l1.835-1.45z"></path>
      </svg>
    </div>
  </div>

  <div v-if="sentiment && !isLoading" class="mt-4 p-4 bg-gray-100 dark:bg-gray-800 rounded-md">
    <Sentiment :sentiment="sentiment" />
  </div>
</div>

`,
    components: { Sentiment },
    setup() {
        const isRecording = ref(false);
        const isLoading = ref(false);
        const audioRecorder = new AudioRecorder();
        const isPlaying = ref(false);
        const sentiment = ref(null);
        const transcript = ref("");
        const client = useClient();  // Using ServiceStack's client

        const toggleRecording = async () => {
            if (!navigator.mediaDevices?.getUserMedia) {
                alert('getUserMedia() is not supported in your browser')
                return;
            }

            if (isRecording.value) {
                isRecording.value = false;
                
                const audio = await audioRecorder.stop();
                audio.addEventListener('playing', e => isPlaying.value = true);
                audio.addEventListener('pause', e => isPlaying.value = false);
                audio.addEventListener('ended', e => isPlaying.value = false);
                isLoading.value = true;
                // Using FormData to upload audio blob
                const formData = new FormData();
                formData.append('path', audioRecorder.audioBlob, `file.${audioRecorder.audioExt}`);
                const api = await client.apiForm(new CreateRecording({feature:'sentiment'}), formData);
                
                if (api.succeeded) {
                    const text = api.response.transcript;
                    transcript.value = text;

                    // Post transcribed text to another service for order processing
                    const apiChat = await client.api(new CreateChat({
                        feature: 'sentiment',    
                        userMessage: text 
                    }));
                    if (apiChat.succeeded) {
                        console.log(apiChat.response)
                        sentiment.value = apiChat.response;
                    }
                }
                isLoading.value = false;
            } else {
                isRecording.value = true;
                await audioRecorder.start();
            }
        };

        return { isRecording, toggleRecording, sentiment, isLoading,transcript };
    }
};

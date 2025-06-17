const {createApp, ref, computed} = Vue
const {createVuetify} = Vuetify
const vuetify = createVuetify();
// user defined properties
let items = ref([]);  
let isLoading = ref(false);
let messageText = ref('');
let email = ref('');
let threadName = ref('');
// routes
let apiBaseUrl = ref('');
let apiChatRoute = ref('');
let saveChatRoute = ref('');
let generateThreadRoute = ref('');
const saveChatUrl = computed(() => 
    apiBaseUrl.value + apiChatRoute.value + '/' + saveChatRoute.value);
const getThreadUrl = computed(() => 
    apiBaseUrl.value + apiChatRoute.value + '/' + generateThreadRoute.value);
// vuejs mechanics
const app = createApp({
    setup() {
        return {
            items,
            isLoading,
            messageText,
            email,
            //routes
            apiBaseUrl,
            apiChatRoute,
            generateThreadRoute,
            threadName,
            saveChatUrl,
            getThreadUrl,
            // methods
            sendChatMessage,
            loadDefaultData,
            saveChatRoute,
            createNewThread            
        }
    },
    mounted() {
        console.log('mounted and loading default data...');   
        loadDefaultData();        
    }
});
app.use(vuetify).mount('#app');
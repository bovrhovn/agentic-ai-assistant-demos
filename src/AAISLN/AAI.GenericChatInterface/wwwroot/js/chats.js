const {createApp, ref} = Vue
const {createVuetify} = Vuetify
const vuetify = createVuetify();
// user defined properties
let items = ref([]);  
let isLoading = ref(false);
let messageText = ref('');
let email = ref('');
let threadName = ref('');
let apiBaseUrl = ref('');
let saveChatRoute = ref('');
// vuejs mechanics
const app = createApp({
    setup() {
        return {
            createNewThread,
            items,
            isLoading,
            messageText,
            email,
            apiBaseUrl,
            threadName,
            sendChatMessage,
            loadDefaultData,
            saveChatRoute            
        }
    },
    mounted() {
        console.log('mounted and loading default data...');   
        loadDefaultData();        
    }
});
app.use(vuetify).mount('#app');
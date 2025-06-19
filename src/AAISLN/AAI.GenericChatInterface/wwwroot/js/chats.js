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
let loadThreadItemsRoute = ref('');
let generateThreadRoute = ref('');
//full routes
const saveChatUrl = computed(() =>
    apiBaseUrl.value + apiChatRoute.value + '/' + saveChatRoute.value);
const getThreadUrl = computed(() =>
    apiBaseUrl.value + apiChatRoute.value + '/' + generateThreadRoute.value);
const getLoadThreadItemsUrl = computed(() =>
    apiBaseUrl.value + apiChatRoute.value + '/' + loadThreadItemsRoute.value);
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
            createNewThread,
            loadThreadData
        }
    },
    mounted() {
        console.log('mounted and loading default data...');
        loadDefaultData();
    }
});
app.use(vuetify).mount('#app');

function loadThreadData(){
    console.log('loadThreadData function with threadName '+ threadName.value + ' and called to url ' + getLoadThreadItemsUrl.value);
    isLoading.value = true;
    const getUrl = getLoadThreadItemsUrl.value + "/" + threadName.value;
    fetch(getUrl, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        if (!response.ok) {
            console.log("There has been an error while fetching data from the server.");
            isLoading.value = false;
            return Promise.reject(response);
        }
        return response.json(); // Parse the JSON from the response
    }).then(response => {
        console.log('Message received successfully:', response);
        response.forEach(msg => {
            items.value.push({
                id: msg.id,
                parentId: msg.parentId,
                text: msg.text,
                sender: msg.sender,
                timeStamp: msg.timeStamp
            });
        });
        isLoading.value = false;
    }).catch(error => {
        console.error('Unable to get messages for thread from service.', error);
        isLoading.value = false;
    });
}

function sendChatMessage() {
    console.log('sendChatMessage function called');
    isLoading.value = true;
    let parentId = '';
    if (items.value.length > 0) {
        parentId = items.value[items.value.length - 1].id;
        console.log('Parent ID:', parentId);
    } else {
        console.log('No previous messages, no parent ID.');
    }
    
    let sendMessageItem = {
        email: email.value,
        text: messageText.value,
        threadName: threadName.value,
        parentId: parentId
    };    
    fetch(saveChatUrl.value, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(sendMessageItem)
    }).then(response => {
        if (!response.ok) {
            console.log("There has been an error while fetching data from the server.");
            isLoading.value = false;            
            return Promise.reject(response);
        }        
        return response.json(); // Parse the JSON from the response
    }).then(response => {
        console.log('Message sent successfully:', response);
        response.forEach(msg => {
            items.value.push({
                id: msg.id,
                parentId: msg.parentId,
                text: msg.text,
                sender: msg.sender,
                timeStamp: msg.timeStamp
            });
        });       
        isLoading.value = false;
    }).catch(error => {
        console.error('Unable to get message from service.', error);
        isLoading.value = false;        
    });

    messageText.value = '';
}

function createNewThread() {
    console.log('clearForm function called');
    isLoading.value = true;
    messageText.value = '';
    fetch(getThreadUrl.value)
        .then(response => {
            if (!response.ok) {
                console.log("There has been an error while fetching data from the server.");
                isLoading.value = false;
                items.value = [];
                return Promise.reject(response);
            }
            return response.json(); // Parse the JSON from the response
        }).then(data => {
        console.log('Thread name generated successfully:', data);
        threadName.value = data;
        isLoading.value = false;
        items.value = [];
    }).catch(error => {
        console.error('Unable to generate thread names.', error);
        isLoading.value = false;
        items.value = [];
    });
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
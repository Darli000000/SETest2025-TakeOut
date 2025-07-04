<script setup>  
import { ref, onMounted, watch,onBeforeUnmount} from 'vue'; 
import { ElMessage,ElMessageBox } from "element-plus"; 
import { useRouter } from 'vue-router';
import { provide } from 'vue';
import { useStore } from "vuex"  
import { getOrderCoupon,getOrderDishes,GetAddressByAddressId,getMerchantsInfo,GetCouponInfo,userInfo,PurchaseOrder,deleteOrder } from '@/api/user'
import { getDishInfo,getOrdersToHandle,merchantInfo,deletePaidOrder ,getFinishedMerOrders} from '@/api/merchant'

const store = useStore()    
const router = useRouter()
const merchant = ref({}); // 初始化商家信息对象  
const isMerchantHome = ref(true); // 页面是否为商家主页 
const showState =ref(1);  //查看订单状态
const orders = ref([]);  //订单列表 
const pendingOrders = ref([]);  //待处理订单
const deliveringOrders = ref([]);  //派送中订单
const completedOrders = ref([]);  //已完成订单
const currentOrder = ref({});  //当前订单
const isOrderInfo = ref(false);   //订单详情是否显示
let updateInterval = null; // 定时器  

const hover = ref(false); // 添加 hover 状态

onMounted(async() => {  
  // 从 store 中读取用户信息  
  const merchantData = store.state.merchant;
  if(router.currentRoute.value.path !== '/merchant-home')
    isMerchantHome.value = false; // 页面是否为商家主页。这里之所以设置这个是因为子路由会默认渲染父路由的一切渲染内容，所以这里设置一个标识符来控制是否显示父路由内容
  else
    isMerchantHome.value = true; 
  if (merchantData) {  
    merchant.value = merchantData;  
    const res=await merchantInfo(merchant.value.MerchantId);
    merchant.value=res.data;
    console.log('商家信息',merchant.value);
  } else {  
    // 用户未登录，跳转到登录页面  
    router.push('/login');
  }  

  await renewOrders();
  updateInterval = setInterval(renewOrders, 10000); // 每10秒更新订单
});  
onBeforeUnmount(() => {  
    if (updateInterval) {  
        clearInterval(updateInterval); // 清除定时器  
    }  
});  

const renewOrders = async() => {  //更新order信息
    try{
        const preOrders=orders.value;
        const preOrderIds = new Set(preOrders.filter(order => order.state === 1 || order.state === 2).map(order => order.orderId)); 
        const ordersData = await getOrdersToHandle(merchant.value.merchantId);
        if(ordersData.data===40000)
        {
          ElMessage.success('无订单');
          orders.value=[];
          pendingOrders.value=[];
          deliveringOrders.value=[];
          completedOrders.value=[];
          return;
        }
        //orders.value = ordersData.data;
        // 给每个订单添加 hover 属性
        orders.value = ordersData.data.map(order => ({
          ...order,
          hover: false,
        }));
        const newOrderIds = new Set(orders.value.filter(order => order.state === 1 || order.state === 2).map(order => order.orderId)); 
        for (const orderId of newOrderIds) {  
            if (!preOrderIds.has(orderId)) {  
                ElMessage.success('您有新的订单,请及时处理');  
                speak('您有新的订单，请及时处理');
                break; // 找到一个新订单后可以退出循环  
            }  
        }  
        for(let i=0;i<orders.value.length;i++){
            const addressData= await GetAddressByAddressId(orders.value[i].addressId);
            orders.value[i].address=addressData.data;
            const orderDishesData = await getOrderDishes(orders.value[i].orderId);
            orders.value[i].dishes=orderDishesData.data;
            const orderCouponData = await getOrderCoupon(orders.value[i].orderId);
            orders.value[i].coupon=orderCouponData.data;
        }
        pendingOrders.value = orders.value
            .filter(order => order.state === 1)
            .map(order => {  
                // 计算离过期时间  
                const orderCreationTime = new Date(order.orderTimestamp).getTime();
                const currentTime = new Date().getTime();  
                const timeDiff = (currentTime - orderCreationTime-8*60*60*1000);  
                // 如果超过30分钟，返回null，后面会通过filter删除  
                if (timeDiff > 30 * 60 * 1000){
                    return null;}
                // 添加倒计时属性  
                return {   
                    ...order,   
                    countdown: Math.max(0, (30 * 60 * 1000 - timeDiff) / 1000)  // 剩余秒数  
                };  
            })  
            .filter(order => order!== null); // 过滤掉null  
        deliveringOrders.value = orders.value.filter(order => order.state === 2);
        completedOrders.value = orders.value.filter(order => order.state === 3);
        console.log('订单',orders.value);
        console.log('待处理订单',pendingOrders.value);
        console.log('派送中订单',deliveringOrders.value);
        console.log('已完成订单',completedOrders.value);
    }catch(error){
        if (error.response && error.response.data) {  
            const errorCode = error.response.data.errorCode;  
            if (errorCode === 40000) {  
                ElMessage.error('无订单');  
            }
        }
    }
}
// 语音朗读功能  
const speak = (text) => {  
    const utterance = new SpeechSynthesisUtterance(text);  
    utterance.lang = 'zh-CN'; // 设置语言为中文  
    speechSynthesis.speak(utterance);  
}; 
// 实现倒计时  
const updateCountdowns = () => {  
    for(const order of pendingOrders.value) {  
        if (order.countdown > 0) {  
            order.countdown -= 1; // 每秒减少1  
        }  
    }
    // 刷新未支付订单列表，删除已经过期的订单 
    pendingOrders.value = pendingOrders.value.filter(order => order.countdown > 0);  
};  

setInterval(updateCountdowns, 1000); // 每秒更新倒计时  
const enterOrderInfo = async(order) => {
    currentOrder.value = order;
    for (const dish of currentOrder.value.dishes) { 
        const dishData = await getDishInfo(dish.merchantId,dish.dishId);
        dish.dishInfo = dishData.data;
    };  
    if(currentOrder.value.coupon){
        const couponData=await GetCouponInfo(currentOrder.value.coupon.couponId);
        currentOrder.value.coupon.couponInfo=couponData.data;
    }
    console.log('当前订单',currentOrder.value);
    isOrderInfo.value = true;
}
const leaveOrderInfo = () => {
    isOrderInfo.value = false;
    currentOrder.value = {};
}
const cancelOrder = async() => {
  try {  
        await ElMessageBox.confirm('是否已与顾客协商，确认要删除该订单吗?', '提示', {  
            confirmButtonText: '确定',  
            cancelButtonText: '取消',  
            type: 'warning'  
        });  
        // 用户点击了"确定"，执行删除操作  
        await deletePaidOrder(currentOrder.value.orderId);  
        renewOrders();
        isOrderInfo.value = false;
        currentOrder.value = {};
        ElMessageBox.success('删除成功!');  
        renewOrders();
    } catch (error) {  
        // 用户点击了"取消"，或者出现其他错误  
        if (error !== 'cancel') {  
            ElMessageBox.error('删除失败!');  
        }  
    }  
}
// 监视路由变化  
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/merchant-home') && newPath !== '/merchant-home/dish' && newPath !== '/merchant-home/personal'&& newPath !== '/merchant-home/specialOffer') {  
            isMerchantHome.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isMerchantHome.value = false; // 进入子路由时隐藏  
        } 
        // 保存状态到 localStorage  
        localStorage.setItem('isMerchantHome', isMerchantHome.value);  
    }  
);  

//跳转回主页
const goBack = () => {  
    router.push('/merchant-home');  
    isMerchantHome.value = true;  
};  

// 跳转到菜单  
const goToMenu = () => { 
    router.push('/merchant-home/dish');  
    isMerchantHome.value = false; // 进入菜单页面时隐藏欢迎信息和按钮  
};  

// 跳转到个人信息  
const goToPersonal = () => { 
    router.push('/merchant-home/personal');  
    isMerchantHome.value = false; // 进入个人信息页面时隐藏欢迎信息和按钮  
};  

// 跳转到满减活动  
const goToSpecialOffer = () => { 
    router.push('/merchant-home/specialOffer');  
    isMerchantHome.value = false; // 进入满减活动页面时隐藏欢迎信息和按钮  
};  

// 提供 merchant 对象 给其它子网页 
provide('merchant', merchant); 
provide('isMerchantHome', isMerchantHome); 
function formatDateTime(time) { 
    const date = new Date(time); 
    if (isNaN(date.getTime())) { 
        return null; // 或者处理无效日期的逻辑  
    } 
    const year = date.getFullYear(); 
    const month = String(date.getMonth() + 1).padStart(2, '0'); // 月份从0开始  
    const day = String(date.getDate()).padStart(2, '0'); 
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0'); 
    const seconds = String(date.getSeconds()).padStart(2, '0'); 

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`; 
}
</script>  

<template>
    <!-- 左侧导航栏 在dish和personal界面下出现主页按钮的虚影，不知道是哪里的bug-->
    <nav class="sidebar">
      <slot class="sidebar-content">
        <img class="sidebar-img" src="@\assets\my_logo.png" alt="logo"/>
        
        <button class="sidebar-button" @click="goBack">
          <img src="@\assets\merchant_home.png" alt="主页"/>
          <span>主页</span>
        </button>
        
        <button class="sidebar-button" @click="goToMenu">
          <img src="@\assets\merchant_menu.png" alt="菜单"/>
          <span>本店菜单</span>
        </button>

        <button class="sidebar-button" @click="goToSpecialOffer">
          <img src="@\assets\merchant_specialOffer.png" alt="满减活动"/>
          <span>满减活动</span>
        </button>
        
        <button class="sidebar-button" @click="goToPersonal">
          <img src="@\assets\merchant_personal.png" alt="个人信息"/>
          <span>个人信息</span>
        </button>

      </slot>
      <router-view /> <!-- 渲染子路由 -->
    </nav>

    <!-- 页面内容区域 #Q# 营收尚不确定是否需要添加，根据后续进度调整-->
    <div  v-if="isMerchantHome" class="content">
      <header v-if="!isOrderInfo">
        <!-- #Q# 考虑将MerchantId改为MerchantName -->
        <span class="welcome-text">欢迎&nbsp;{{ merchant.merchantName }}!</span>
        <!-- <span class="revenue-text">今日营收：{{ todayRevenue }}元    </span>-->
      </header>
      <!-- 当前订单 实际变量根据平台方订单结构调整 -->
      <div class="orders" v-if="!isOrderInfo">
        <h2>
          <label 
            @click="showState=1" 
            :class="{ active: showState === 1 }"
          >待处理订单</label>&nbsp;&nbsp;
          <label 
            @click="showState=2" 
            :class="{ active: showState === 2 }"
          >运送中订单</label>&nbsp;&nbsp;
          <label 
            @click="showState=3" 
            :class="{ active: showState === 3 }"
          >已完成订单</label>&nbsp;&nbsp;
        </h2>

        <div class="orders-container">
          <div class="orders-scroll" v-if="showState===1">
            <div 
              class="order-item" 
              v-for="(order, index) in pendingOrders" 
              :key="index"
              @click="enterOrderInfo(order);order.hover = false"
              @mouseover="order.hover = true"
              @mouseleave="order.hover = false"
            :style="{ backgroundColor: order.hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
            >
              <div>订单号：{{order.orderId}}</div>
              <div>订单总价：{{order.price}}元</div>
              <div>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</div>
              <div>等待骑手接单:{{ Math.floor(order.countdown/60) }}:{{ Math.floor(order.countdown%60) }}</div>
            </div>
          </div>
          <div class="orders-scroll" v-if="showState===2">
            <div 
              class="order-item" 
              v-for="(order, index) in deliveringOrders" 
              :key="index"
              @click="enterOrderInfo(order);order.hover = false"
              @mouseover="order.hover = true"
              @mouseleave="order.hover = false"
            :style="{ backgroundColor: order.hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
            >
              <div>订单号：{{order.orderId}}</div>
              <div>订单总价：{{order.price}}元</div>
              <div>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</div>
              <!--<div>预计送达时间: {{ order.expectedTimeOfArrival }}</div>-->
              
            </div>
          </div>
      
          <div class="orders-scroll" v-if="showState===3">
            <div 
              class="order-item" 
              v-for="(order, index) in completedOrders" 
              :key="index"
              @click="enterOrderInfo(order);order.hover = false"
              @mouseover="order.hover = true"
              @mouseleave="order.hover = false"
            :style="{ backgroundColor: order.hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
            >
              <div>订单号：{{order.orderId}}</div>
              <div>订单总价：{{order.price}}元</div>
              <div>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</div>
              <div>送达时间:{{ formatDateTime(order.realTimeOfArrival) }}</div>
            </div>
          </div>
        </div>
      </div>
      <!-- #Q# 菜单预览 还没做完
      <div class="menu-preview">
        <h2>菜单预览</h2>
        <img src="@\assets\merchant_menu.png" alt="菜单"/>
      </div>-->
      <div v-if="isOrderInfo" class = "order-info">
        <span>订单详情</span>
        <p>订单号：{{currentOrder.orderId}}</p>
        <p>状态：{{ currentOrder.state===0?'未支付':currentOrder.state===1?'待处理':currentOrder.state===2?'派送中':currentOrder.state===3?'已完成':'未知状态' }}</p>
        <div>
            <p>{{ currentOrder.address.contactName }}&nbsp;&nbsp;{{ currentOrder.address.phoneNumber }}</p>
             {{ currentOrder.address.userAddress }}&nbsp;{{ currentOrder.address.houseNumber}}
        </div>
        <p>{{merchant.merchantName}}：</p>
        <p>联系电话：{{ merchant.contact }}</p>
        <p>商家地址：{{ merchant.merchantAddress }}</p>
        <ul>
            <li v-for="(dish,index) in currentOrder.dishes" :key="index">
                <p><img :src="dish.dishInfo.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
                    {{ dish.dishInfo.dishName }} ×{{dish.dishNum}}
                </p>
            </li>
        </ul>
        <p>优惠券：{{currentOrder.coupon?currentOrder.coupon.couponInfo.couponName:'无'}} &nbsp;{{currentOrder.coupon?'满'+currentOrder.coupon.couponInfo.minPrice+'减'+currentOrder.coupon.couponInfo.couponValue+'元':''}}</p>
        <p>总价：{{currentOrder.price}}元</p>
        <button @click="leaveOrderInfo()" class = "back">返回</button>
        <button v-if="currentOrder.state===1||currentOrder.state===2" @click="cancelOrder()" class = "cancel">取消订单</button>
      </div>
    </div>

</template>

<script>
/*
export default {
  // #Q# 以下内容为网页排版中的测试用例，实际场景需要调用平台方订单数据动态获取
  data() {
      return {
          username: '用户名', // 用户名
          orders: [
              { id: 1, totalAmount: 100, orderTime: '2023-10-01 10:00', progress: '制作中', delivered: false, rating: null },
              { id: 2, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 3, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 4, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 5, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 6, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
          ], // 订单数据
          todayRevenue: 250 // 今日营收数据
      };
  },
};*/
</script>

<style scoped lang = "scss">
h2 {
  display: flex;
  align-items: center;
  justify-content: space-between; /* 均匀分布标签 */
  font-size: 20px;
  margin: 20px;
  padding: 5px;
  padding-left: 100px;
  background-color: #ffcc00;
  border-radius: 40px;
  border: 2px solid #000000;
}

.sidebar {
  width: 5vw;
  background: linear-gradient(to bottom, #ffcc00, #69b1f848);
  padding: 15px;
  height: 100vh;
  position: fixed;
  top: 0;
  left: 0;
}

.sidebar-img {
  width: 100%;
  height: auto;
  margin-bottom: 15px;
}

.sidebar-button {
  display: block;
  width: 50px;
  height: auto;
  margin-bottom: 20px;
  padding: 0;
  text-align: center;
  border: none;
  background-color: transparent;
  cursor: pointer;
  
}

.sidebar-button img {
    width: 100%;
    height: auto;
}

.sidebar-button span {
  display: block;
  font-size: 12px;
  text-align: center;
}

.sidebar-button.active {
  color: #0f628b;
}

.sidebar-content button:hover {
  background-color: #3686d748;
}

label {
  cursor: pointer;
  color: #000000; /* 默认颜色 */
  background-color: #ffcc00; /* 选中时的颜色 */
  border:2px,solid,#ffcc00;
  padding:10px;
  border-radius:40px;
}

.normal-button {
  background-color: #ffcc00;
  color:black;
}

label.active {
  background-color: rgb(255, 255, 255); /* 选中时的颜色 */
  border:2px,solid,#000000;
  padding:10px;
  border-radius:40px;
}

.orders{
  margin-bottom: 30px;
}

.orders-scroll {
  max-height: 390px; /* 设置订单区域的最大高度 */
  display: flex;
  flex-direction: column;
  overflow-y: auto; /* 使订单区域可以滚动 */
  margin-left: 10px;
  margin-top:13px;
}

/* 隐藏滚动条 */
.orders-scroll::-webkit-scrollbar {
    width: 12px;
}

/* 滚动条轨道 */
.orders-scroll::-webkit-scrollbar-track {
    background: #ffd666;
}
/* 滚动条滑块 */
.orders-scroll::-webkit-scrollbar-thumb {
    background-color: #ffd666;
    border-radius: 10px;
    border: 2px solid #000000;
}

.order-item {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  padding: 10px;
  margin: 0 30px;     
  margin-bottom: 10px;
  border: 2px solid #ffee00;
  border-radius: 8px;
  background-color: #f9f9f9;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.orders-container{
    background-color: #ffd666;
    border:2px solid black;
    border-radius: 20px;
    padding:5px;
    
    max-height: 410px; /* 设置订单区域的最大高度 */
    min-height: 410px;
    display: flex;
    flex-direction: column;
    overflow-y: auto; /* 使订单区域可以滚动 */
    
    margin-left: 20px;
    margin-right: 20px;
    margin-bottom:10px;
}

.order-item div{
  margin-left: 20px;
  font-size: 16px;
  flex:1 1 50%;
  box-sizing: border-box;
}


.welcome-text {
  font-size: 35px;
  margin-left: 15px;
  color: #000000;
  font-weight: bold;
}

.menu-preview { 
  margin-bottom: 20px;
}

.order-info {
  padding: 20px;
  background-color: #f9f9f9;
  border: 2px solid #000000;
  border-radius: 20px;
  margin-right:30px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);

  span {
    font-size: 24px;
    font-weight: bold;
    margin-bottom: 15px;
    display: block;
  }

  p {
    font-size: 16px;
    margin: 10px 0;
    
    &.status {
      color: #ffcc00;
      font-weight: bold;
    }
    
    &:nth-child(odd) {
      background-color: #fff;
    }
    
    &:nth-child(even) {
      background-color: #f1f1f1;
    }
  }

  .contact-info {
    margin-top: 15px;
    p {
      margin: 5px 0;
    }
  }

  ul {
    list-style: none;
    padding: 0;
    margin: 20px 0;
    
    li {
      display: flex;
      align-items: center;
      padding: 10px 0;
      
      img {
        margin-right: 10px;
        border-radius: 4px;
      }
    }
  }

  .coupon-info {
    margin: 20px 0;
    font-style: italic;
  }

  .total-price {
    font-size: 18px;
    font-weight: bold;
  }

  button {
    padding: 10px 20px;
    border: none;
    border-radius: 10px;
    cursor: pointer;
    margin-right: 10px;

    &.back {
      background-color: #ffcc00;
      color: black;
      
      &:hover {
        background-color: #ffb700;
      }
    }

    &.cancel {
      background-color: #f44336;
      color: white;

      &:hover {
        background-color: #d32f2f;
      }
    }
  }
}
</style>

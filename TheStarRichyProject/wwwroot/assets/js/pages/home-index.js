const language = localStorage.getItem("language");
let allMessages = [];
let currentMessageIndex = 0;
let messageModal = null;

// Function to get cookie by name
function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");

    //console.log(parts.pop().split(";").shift());

    if (parts.length == 2) return parts.pop().split(";").shift();
    return null;
}

function getColor(index) {
    // อาร์เรย์ของคลาส CSS (เรียงตามที่ระบุ)
    const colorClasses = [
        'color-blue',    // ฟ้า
        'color-golden',  // เหลืองทอง
        'color-navy',    // น้ำเงิน
        'color-purple',  // ม่วง
        'color-green',   // เขียว
        'color-red',     // แดง
        'color-pink',    // ชมพู
        'color-gray',    // เทา
        'color-brown',   // น้ำตาล
        'color-black'    // ดำ
    ];
    return colorClasses[index];
}

// ฟังก์ชันตัดข้อความ
function truncateText(text, maxLength = 100) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

var memberinfo;
$(document).ready(function () {
    const fp = flatpickr(".dash-filter-picker", {
        mode: "range",
        dateFormat: "d M, Y",
        defaultDate: [
            // Start of the current month
            new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            // End of the current month
            new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
        ]
    });

    $('#countdownModal').modal('show');

    // Load member info from API
    async function loadMemberInfo() {
        /*if (!token) {
            window.location.href = '/Auth/Login';
            return;
        }*/


        try {
            const response = await fetch('/home/GetMemberInfo', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //const data = await response.json();
            const member = await response.json();
            if (member) {
                memberinfo = member;
                //console.info(member);
                //console.log('JWT Token:', member);
                // Update UI with member data
                $('#user-profile-image').attr('src', member.memberPositionPicture || 'N/A');
                $('#membercode').text(member.membercode || 'N/A');
                $('#bussinessName').text(member.bussinessName || 'N/A');
                $('#personalPV').text(formatNumberWithComma(member.personalPV || '0'));
                $('#positionName').text(member.positionName || 'N/A');//position1
                $('#thaiName').text(member.thaiName || 'N/A');//thaiName
                $('#sponsername').text(member.sponsername || 'N/A');//sponsername
                $('#bwdLeftPV').text(formatNumberWithComma(member.bwdLeftPV || '0'));
                $('#bwdRightPV').text(formatNumberWithComma(member.bwdRightPV || '0'));
                $('#presentLeftPV').text(formatNumberWithComma(member.presentLeftPV || '0'));
                $('#presentRightPV').text(formatNumberWithComma(member.presentRightPV || '0'));
                $('#newLeftPV').text(formatNumberWithComma(member.newLeftPV || '0'));
                $('#newRightPV').text(formatNumberWithComma(member.newRightPV || '0'));
                $('#totalLeftBalanceTeam').text(formatNumberWithComma(member.totalLeftBalanceTeam || '0'));
                $('#totalRightBalanceTeam').text(formatNumberWithComma(member.totalRightBalanceTeam || '0'));
                $('#totalNewMonthLeftPV').text(formatNumberWithComma(member.totalNewMonthLeftPV || '0'));
                $('#totalNewMonthRightPV').text(formatNumberWithComma(member.totalNewMonthRightPV || '0'));

                $('#totalBonus').text(formatNumberWithComma(member.totalBonus || '0'));
                $('#totalBalance').text(formatNumberWithComma(member.totalBalance || '0'));
                $('#leftCountActive').text(Number(member.leftCountActive || '0').toLocaleString("en-US"));
                $('#rightCountActive').text(Number(member.rightCountActive || '0').toLocaleString("en-US"));
                $('#ewallet').text(formatNumberWithComma(member.ewallet || '0'));
                $('#holdPV').text(formatNumberWithComma(member.holdPV || '0'));
                $('#qualifyDate').text(member.qualifyDate || 'N/A');
                $('#firstQdate').text(member.firstQdate || 'N/A');
                $('#currentMonth').text(member.currentMonth || 'N/A');
                $('#nextCMonth').text(member.nextCMonth || 'N/A');
                $('#currentMonthQualifyPV').text(formatNumberWithComma(member.currentMonthQualifyPV || '0'));
                $('#currentMonth1').text(member.currentMonth1 || 'N/A');
                $('#currentmonthStatus').text(member.currentMonthStatus || 'N/A');
                $('#lastmonthQualifyPV').text(formatNumberWithComma(member.lastmonthQualifyPV || '0'));
                $('#lastCMonth').text(member.lastCMonth || 'N/A');
                $('#lastmonthStatus').text(member.lastMonthStatus || 'N/A');
                $('#leftURL').html(member.leftURL || 'N/A');
                $('#leftURL').attr('href', member.leftURL || 'N/A');
                $('#rightURL').html(member.rightURL || 'N/A');
                $('#rightURL').attr('href', member.rightURL || 'N/A');
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    //Member Team Total Position Package
    async function loadMemberTeamTotalPositionPackage() {
        try {
            const response = await fetch('/home/GetMemberTeamTotalPositionPackage', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //{Membercode: 'S00000029', M24_X8: '01', Positionname: 'Star', Totalmember: 7}
            //const data = await response.json();
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#positionPackageContainer');

                let totalMembersSum = 0; // ตัวแปรสำหรับเก็บผลรวม

                // คำนวณผลรวมก่อน
                $.each(data, function (index, item) {
                    const totalmember = parseInt(item.Totalmember); // แปลงเป็นตัวเลข
                    if (totalmember) totalMembersSum += totalmember; // เพิ่มเฉพาะเมื่อเป็นตัวเลขที่ถูกต้อง
                });

                // สร้าง HTML สำหรับผลรวมและเพิ่มไว้ด้านบน
                const totalHtml = `
                    <div class="row d-flex align-items-center justify-content-center">
                        <div class="col-xl-6">
                            <h6><strong class="text-danger" data-key="t-totalmember">จำนวนสมาชิกทั้งหมด</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${Number(totalMembersSum || '0').toLocaleString("en-US")}</strong>
                            </div>
                        </div>
                    </div>
                `;

                // เพิ่มผลรวมไว้ด้านบนสุดของ container
                $positionContainer.prepend(totalHtml);

                $.each(data, function (index, item) {
                    const positionname = item.Positionname;
                    const totalmember = item.Totalmember;
                    let positionkey = item.Positionname;
                    let positioncolor = "text-success";

                    if (positionname === "No Position") {
                        positionkey = "noposition";
                    } else if (positionname === "Star") {
                        positionkey = "star";
                        positioncolor = "text-info";
                    } else if (positionname === "Silver Star") {
                        positionkey = "silverstar";
                        positioncolor = "text-pink";
                    } else if (positionname === "Gold Star") {
                        positionkey = "goldstar";
                        positioncolor = "text-warning";
                    }

                    // สร้าง HTML สำหรับ row
                    const rowHtml = `
                        <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                            <div class="col-xl-6">
                                <h6><strong class="${positioncolor}" data-key="t-${positionkey}">${positionname}</strong></h6>
                            </div>
                            <div class="col-xl-6">
                                <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                    <strong>${Number(totalmember || '0').toLocaleString("en-US")}</strong>
                                </div>
                            </div>
                        </div>
                    `;

                    // เพิ่ม row เข้าใน container
                    $positionContainer.append(rowHtml);
                });
                //console.info(memberinfo);
                $positionContainer.append(`<hr>`);
                $positionContainer.append(`
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-secondary" data-key="t-totalmember_buy">จำนวนคนที่ซื้อสินค้า</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${Number(memberinfo.totalmember_buy || '0').toLocaleString("en-US")}</strong>
                            </div>
                        </div>
                    </div>
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-danger" data-key="t-totalmember_notbuy">จำนวนคนที่ไม่ซื้อสินค้า</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${Number(memberinfo.totalmember_notbuy || '0').toLocaleString("en-US")}</strong>
                            </div>
                        </div>
                    </div>
                `);
                $positionContainer.append(`<hr><h4 class="card-title mb-0 flex-grow-1 text-secondary text-center">สรุปยอดซื้อทีมงานในเดือนปัจจุบัน</h4><hr>`);
                $positionContainer.append(`
                    <div class="row d-flex align-items-center justify-content-center">
                        <div class="col-xl-6">
                            <h6><strong class="text-pink" data-key="t-totalmember_buy_month">จำนวนคนที่รักษายอด</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${Number(memberinfo.totalmember_buy_month || '0').toLocaleString("en-US")}</strong>
                            </div>
                        </div>
                    </div>
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-success" data-key="t-amountBUY_Month">ยอดเงิน</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${formatNumberWithComma(memberinfo.amountBUY_Month || '0')}</strong>
                            </div>
                        </div>
                    </div>
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-danger" data-key="t-pvbuY_Month">จำนวนคะแนน</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${formatNumberWithComma(memberinfo.pvbuY_Month || '0')}</strong>
                            </div>
                        </div>
                    </div>
                `);

                $positionContainer.append(`<hr><h4 class="card-title mb-0 flex-grow-1 text-secondary text-center">สรุปยอดขายทีมงานทั้งหมด</h4><hr>`);
                $positionContainer.append(`
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-success" data-key="t-amountBUY">ยอดเงิน</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${formatNumberWithComma(memberinfo.amountBUY || '0')}</strong>
                            </div>
                        </div>
                    </div>
                    <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                        <div class="col-xl-6">
                            <h6><strong class="text-danger" data-key="t-pvbuy">จำนวนคะแนน</strong></h6>
                        </div>
                        <div class="col-xl-6">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${formatNumberWithComma(memberinfo.pvbuy || '0')}</strong>
                            </div>
                        </div>
                    </div>
                `);
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    //Member Team Total Position Ranking
    async function loadMemberTeamTotalPositionRanking() {
        try {
            const response = await fetch('/home/GetMemberTeamTotalPositionRanking', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //{Membercode: 'S00000029', M24_X8: '01', Positionname: 'Star', Totalmember: 7}
            //const data = await response.json();
            const data = await response.json();
            if (data) {
                //console.info(memberinfo);
                const $positionContainer = $('#positionRankingContainer');

                // สร้าง HTML สำหรับผลรวมและเพิ่มไว้ด้านบน
                const totalHtml = `
                    <div class="row d-flex align-items-center justify-content-center">
                        <div class="col-xl-7">
                            <h6><strong class="text-danger" data-key="t-totalmember">จำนวนตำแหน่งคุณวุฒิทั้งหมด</strong></h6>
                        </div>
                        <div class="col-xl-5">
                            <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                <strong>${Number(memberinfo.sumranking || '0').toLocaleString("en-US")}</strong>
                            </div>
                        </div>
                    </div>
                `;

                // เพิ่มผลรวมไว้ด้านบนสุดของ container
                $positionContainer.prepend(totalHtml);

                $.each(data, function (index, item) {
                    const levelcode = item.LevelCode;
                    const positionname = item.Positionname;
                    const totalmember = item.Totalmember;
                    let positionkey = item.Positionname;
                    let positioncolor = "text-success";

                    if (positionname === "Super Star") {
                        positionkey = "superstar";
                        positioncolor = "text-success";
                    } else if (positionname === "Double Gold Star") {
                        positionkey = "doublegoldstar";
                        positioncolor = "text-info";
                    } else if (positionname === "Triple Gold Star") {
                        positionkey = "triplegoldstar";
                        positioncolor = "text-purple";
                    } else if (positionname === "Platinum Star") {
                        positionkey = "platinumstar";
                        positioncolor = "text-warning";
                    } else if (positionname === "Pearl Star") {
                        positionkey = "pearlstar";
                        positioncolor = "text-gray";
                    } else if (positionname === "Ruby Star") {
                        positionkey = "triplegoldstar";
                        positioncolor = "text-black";
                    } else if (positionname === "Sapphire Star") {
                        positionkey = "sapphirestar";
                        positioncolor = "text-success";
                    } else if (positionname === "Emerald Star") {
                        positionkey = "emeraldstar";
                        positioncolor = "text-info";
                    } else if (positionname === "Diamond Star") {
                        positionkey = "diamondstar";
                        positioncolor = "text-purple";
                    } else if (positionname === "Executive Diamond Star") {
                        positionkey = "executivediamondstar";
                        positioncolor = "text-warning";
                    } else if (positionname === "Double Diamond Star") {
                        positionkey = "doublediamondstar";
                        positioncolor = "text-brown";
                    } else if (positionname === "Triple Diamond Star") {
                        positionkey = "triplediamondstar";
                        positioncolor = "text-pink";
                    } else if (positionname === "Crown Diamond Star") {
                        positionkey = "crowndiamondstar";
                        positioncolor = "text-danger";
                    }

                    if (levelcode !== '00') {
                        // สร้าง HTML สำหรับ row
                        const rowHtml = `
                            <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                                <div class="col-xl-7">
                                    <h6><strong class="${positioncolor}" data-key="t-${positionkey}">${positionname}</strong></h6>
                                </div>
                                <div class="col-xl-5">
                                    <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                        <strong>${Number(totalmember || '0').toLocaleString("en-US")}</strong>
                                    </div>
                                </div>
                            </div>
                        `;
                        // เพิ่ม row เข้าใน container
                        $positionContainer.append(rowHtml);
                    }
                });
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    //Member Team By Region
    async function loadMemberTeamByRegion() {
        try {
            const response = await fetch('/home/GetMemberTeamByRegion', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //{Membercode: 'S00000029', M24_X8: '01', Positionname: 'Star', Totalmember: 7}
            //const data = await response.json();
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#regionContainer');

                $.each(data, function (index, item) {
                    const regionid = item.Region_ID;
                    const totalmember = item.TotalMember_Region;
                    const regionname = item.Region_name;
                    let regioncolor = "text-danger";

                    if (regionid == 1) {
                        regioncolor = "text-brown";
                    } else if (regionid == 2) {
                        regioncolor = "text-info";
                    } else if (regionid == 3) {
                        regioncolor = "text-success";
                    } else if (regionid == 4) {
                        regioncolor = "text-pink";
                    } else if (regionid == 5) {
                        regioncolor = "text-gray";
                    } else if (regionid == 6) {
                        regioncolor = "text-purple";
                    }

                    // สร้าง HTML สำหรับ row
                    const rowHtml = `
                        <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                            <div class="col-xl-6">
                                <h6><strong class="${regioncolor}">${regionname}</strong></h6>
                            </div>
                            <div class="col-xl-6">
                                <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                    <strong>${Number(totalmember || '0').toLocaleString("en-US")}</strong>
                                </div>
                            </div>
                        </div>
                    `;
                    // เพิ่ม row เข้าใน container
                    $positionContainer.append(rowHtml);
                });
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    //Member Team By Region Buy
    async function loadMemberTeamByRegionBuy() {
        try {
            const response = await fetch('/home/GetMemberTeamByRegionBuy', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //{Membercode: 'S00000029', M24_X8: '01', Positionname: 'Star', Totalmember: 7}
            //const data = await response.json();
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#regionbuyContainer');

                $.each(data, function (index, item) {
                    const regionid = item.Region_ID;
                    const totalmember = item.SUMRegionAmount;
                    const regionname = item.Region_Name;
                    let regioncolor = "text-danger";

                    if (regionid == 1) {
                        regioncolor = "text-brown";
                    } else if (regionid == 2) {
                        regioncolor = "text-info";
                    } else if (regionid == 3) {
                        regioncolor = "text-success";
                    } else if (regionid == 4) {
                        regioncolor = "text-pink";
                    } else if (regionid == 5) {
                        regioncolor = "text-gray";
                    } else if (regionid == 6) {
                        regioncolor = "text-purple";
                    }

                    // สร้าง HTML สำหรับ row
                    const rowHtml = `
                        <div class="row d-flex align-items-center justify-content-center mb-1 mt-1">
                            <div class="col-xl-6">
                                <h6><strong class="${regioncolor}">${regionname}</strong></h6>
                            </div>
                            <div class="col-xl-6">
                                <div class="alert bg-info border-info h6 text-white material-shadow text-center" style="border-radius: var(--vz-border-radius-xl);" role="alert">
                                    <strong>${formatNumberWithComma(totalmember || '0')}</strong>
                                </div>
                            </div>
                        </div>
                    `;
                    // เพิ่ม row เข้าใน container
                    $positionContainer.append(rowHtml);
                });
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    //Member Team Buy Product
    async function loadMemberTeamBuyProduct() {
        try {
            const response = await fetch('/home/GetMemberTeamBuyProduct', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            //{Membercode: 'S00000029', M24_X8: '01', Positionname: 'Star', Totalmember: 7}
            //const data = await response.json();
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#buyproductContainer');

                let tableHtml = '<div class="buy-product-table-wrap"><table class="table table-borderless buy-product-table">';
                $.each(data.slice(0, 10), function (index, item) {
                    const productid = item.Num;
                    const totalmember = item.Quantity;
                    const productname = item.ProductName;
                    let productcolor = getColor(index);
                    let productunit = item.UnitThai;
                    if (language === 'th') productunit = item.UnitThai;
                    else productunit = item.UnitEng;

                    tableHtml += `
                        <tr>
                            <td class="p-1 pb-2"><strong class="${productcolor}">${productid}</strong></td>
                            <td class="p-1"><strong class="${productcolor}">${productname}</strong></td>
                            <td class="p-1"><strong class="${productcolor}">${Number(totalmember || '0').toLocaleString("en-US")} ${productunit}</strong></td>
                        </tr>
                    `;
                });
                tableHtml += '</table></div>';
                // เพิ่ม row เข้าใน container
                $positionContainer.append(tableHtml);

            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }
    //EstimatePosition
    async function loadMemberEstimatePosition() {
        try {
            const response = await fetch('/home/GetMemberEstimatePosition', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            const data = await response.json();
            if (data) {
                const $positionContainer = $('#EstimatePositionContainer');
                let tableHtml = '';
                $.each(data.slice(0, 3), function (index, item) {
                    const productid = item.Num;
                    const totalmember = item.Quantity;
                    const productname = item.ProductName;
                    let productcolor = getColor(index);
                    let productunit = item.UnitThai;
                    if (language === 'th') productunit = item.UnitThai;
                    else productunit = item.UnitEng;

                    tableHtml += `<hr />
                        <div class="col-xl-6">
                            <h6 class="mb-1"><strong class="text-secondary" data-key="t-nextposition">ตำแหน่งที่จะขึ้น</strong></h6>
                            <div class="alert bg-warning border-warning h6 text-white material-shadow text-center mb-1" role="alert">
                                <strong>${(item.EstimatePosistion || 'N/A')}</strong>
                            </div>
                        </div>
                        <div class="col-xl-6">
                            <h6 class="mb-1"><strong class="text-secondary" data-key="t-estweakleg">คะแนนตำแหน่ง</strong></h6>
                            <div class="alert bg-purple border-purple h6 text-white material-shadow text-center mb-1" role="alert">
                                <strong>${formatNumberWithComma(item.EstimatetWeakleg || '0')}</strong>
                            </div>
                        </div>
                        <div class="col-xl-6">
                            <h6 class="mb-1"><strong class="text-secondary" data-key="t-weakleg">คะแนนทำเพิ่มด้านซ้าย</strong></h6>
                            <div class="alert bg-danger border-danger h6 text-white material-shadow text-center mb-1" role="alert">
                                <strong>${formatNumberWithComma(item.LeftEstimate || '0')}</strong>
                            </div>
                        </div>
                        <div class="col-xl-6">
                            <h6 class="mb-1"><strong class="text-secondary" data-key="t-weakleg">คะแนนทำเพิ่มด้านขวา</strong></h6>
                            <div class="alert bg-danger border-danger h6 text-white material-shadow text-center mb-1" role="alert">
                                <strong>${formatNumberWithComma(item.RightEstimate || '0')}</strong>
                            </div>
                        </div>
                    `;
                });
                tableHtml += '';
                // เพิ่ม row เข้าใน container
                $positionContainer.append(tableHtml);
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }
    //Member Team New Buy Product
    async function loadMemberTeamNewBuyProduct() {
        try {
            const response = await fetch('/home/GetMemberTeamNewBuy', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#teamNewBuyContainer');

                let tableHtml = `
                <div class="team-new-buy-table-wrap">
                <table class="table table-borderless team-new-buy-table">
                    <thead class="text-secondary" style="background-color: #e6f3ff;">
                        <tr>
                            <th class="p-2 text-center" data-key="t-no">ลำดับที่</th>
                            <th class="p-2 text-center" data-key="t-levelno">ชั้นที่</th>
                            <th class="p-2" data-key="t-date">วันที่</th>
                            <th class="p-2" data-key="t-name">ชื่อ</th>
                            <th class="p-2 text-ceneter" data-key="t-amount">จำนวนเงิน</th>
                            <th class="p-2 text-ceneter" data-key="t-pv">คะแนน</th>
                        </tr>
                    </thead>
                    <tbody>
                `;

                $.each(data.slice(0, 10), function (index, item) {
                    const productid = item.Num;
                    const level = item.LEVELBUY;
                    const orderdate = new Date(item.OrderDateTime).toLocaleString('th-TH', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                        hour: '2-digit',
                        minute: '2-digit',
                        // ถ้าต้องการวินาทีด้วย:
                        second: '2-digit',
                        hour12: false
                    });
                    const dlname = item.DLName;
                    const amount = Number(item.Amount || '0').toLocaleString("en-US", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                    const pv = Number(item.PV || '0').toLocaleString("en-US", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                    let productcolor = "";

                    tableHtml += `
                        <tr>
                            <td class="p-2 text-secondary text-center"><strong class="${productcolor}">${productid}</strong></td>
                            <td class="p-2 text-secondary text-center"><strong class="${productcolor}">${level}</strong></td>
                            <td class="p-2"><strong class="${productcolor}">${orderdate}</strong></td>
                            <td class="p-2"><strong class="${productcolor}">${dlname}</strong></td>
                            <td class="p-2 text-right"><strong class="${productcolor}">${amount}</strong></td>
                            <td class="p-2 text-right"><strong class="${productcolor}">${pv}</strong></td>
                        </tr>
                    `;
                });
                tableHtml += `
                    </tbody>
                </table>
                </div>`;
                // เพิ่มตารางเข้าใน container
                $positionContainer.append(tableHtml);
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }
    //Member Team New Register
    async function loadMemberTeamNewRegister() {
        try {
            const response = await fetch('/home/GetMemberTeamNewRegister', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#teamNewRegisterContainer');

                let tableHtml = `
                <div class="team-new-register-table-wrap">
                <table class="table table-borderless team-new-register-table">
                    <thead class="text-secondary" style="background-color: rgb(249 235 210) !important;">
                        <tr>
                            <th class="p-2 text-center" data-key="t-no">ลำดับที่</th>
                            <th class="p-2 text-center" data-key="t-levelno">ชั้นที่</th>
                            <th class="p-2" data-key="t-date">วันที่</th>
                            <th class="p-2" data-key="t-name">ชื่อ</th>
                        </tr>
                    </thead>
                    <tbody>
                `;

                $.each(data.slice(0, 10), function (index, item) {
                    const productid = item.Num;
                    const level = item.LEVELTeam;
                    const orderdate = new Date(item.Regisdatetime).toLocaleString('th-TH', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                        hour: '2-digit',
                        minute: '2-digit',
                        // ถ้าต้องการวินาทีด้วย:
                        second: '2-digit',
                        hour12: false
                    });
                    const dlname = item.Name;
                    let productcolor = "";

                    tableHtml += `
                        <tr>
                            <td class="p-2 text-secondary text-center"><strong class="${productcolor}">${productid}</strong></td>
                            <td class="p-2 text-secondary text-center"><strong class="${productcolor}">${level}</strong></td>
                            <td class="p-2"><strong class="${productcolor}">${orderdate}</strong></td>
                            <td class="p-2"><strong class="${productcolor}">${dlname}</strong></td>
                        </tr>
                    `;
                });
                tableHtml += `
                    </tbody>
                </table>
                </div>`;
                // เพิ่มตารางเข้าใน container
                $positionContainer.append(tableHtml);
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }
    //incomeByPeriod
    async function loadincomeByPeriod() {
        try {
            const response = await fetch('/home/GetIncomeByPeriod', {
                method: 'GET',
                credentials: 'include' // ส่ง Cookie ไปด้วย
            });
            const data = await response.json();
            if (data) {
                //console.info(data);
                const $positionContainer = $('#incomeByPeriodContainer');

                let tableHtml = `
                <table class="table table-border">
                    <thead class="text-secondary">
                        <tr>
                            <th class="p-2" width="40%" data-key="t-period">รอบการรับโบนัส</th>
                            <th class="p-2" width="60%" data-key="t-totalbonusperiod">รายได้</th>
                        </tr>
                    </thead>
                    <tbody>
                `;

                $.each(data, function (index, item) {
                    const productid = item.Num;
                    const period = item.period;
                    let periodname = "";
                    let productcolor = "";
                    if (period == "9_months") periodname = "9 เดือน";
                    else if (period == "1_year") periodname = "1 ปี";
                    else if (period == "2_year") periodname = "2 ปี";
                    else if (period == "1_months") periodname = "1 เดือน";
                    else if (period == "2_months") periodname = "2 เดือน";
                    else if (period == "3_months") periodname = "3 เดือน";
                    else if (period == "4_months") periodname = "4 เดือน";
                    else if (period == "5_months") periodname = "5 เดือน";
                    else if (period == "6_months") periodname = "6 เดือน";
                    else if (period == "7_months") periodname = "7 เดือน";
                    else if (period == "8_months") periodname = "8 เดือน";
                    else if (period == "10_months") periodname = "10 เดือน";
                    else if (period == "11_months") periodname = "11 เดือน";
                    tableHtml += `
                        <tr>
                            <td class="p-2 text-secondary"><strong class="${productcolor}">${periodname}</strong></td>
                            <td class="p-2 text-secondary"><strong class="${productcolor}">${formatNumberWithComma(item.TotalBonusPeriod)}</strong></td>
                        </tr>
                    `;
                });
                tableHtml += `
                    </tbody>
                </table>`;
                // เพิ่มตารางเข้าใน container
                $positionContainer.append(tableHtml);
            } else {
                console.error('No token found');
                return null;
            }
        } catch (error) {
            console.error('Error fetching token:', error);
            return null;
        }
    }

    // Business news
    async function loadBusinessNews() {
        try {
            const response = await fetch('/home/GetPopupSlideImages', {
                method: 'GET',
                credentials: 'include'
            });

            const data = await response.json();
            const $newsContainer = $('#newsContainer');
            $newsContainer.empty();

            if (!Array.isArray(data) || data.length === 0) {
                $newsContainer.append('<div class="news-empty">ยังไม่มีข่าวสาร</div>');
                return;
            }

            const basePath = '/assets/images/popup/';
            let newsHtml = '<div class="news-grid">';

            $.each(data.slice(0, 10), function (index, fileName) {
                const imageUrl = encodeURI(`${basePath}${fileName}`);
                newsHtml += `
                    <a class="news-item" href="${imageUrl}" target="_blank" rel="noopener noreferrer">
                        <img src="${imageUrl}" alt="ข่าวสารนักธุระกิจ ${index + 1}" loading="lazy" />
                        <span class="news-item-label">ข่าวสาร ${index + 1}</span>
                    </a>
                `;
            });

            newsHtml += '</div>';
            $newsContainer.append(newsHtml);
        } catch (error) {
            console.error('Error loading business news:', error);
            $('#newsContainer').html('<div class="news-empty">ไม่สามารถโหลดข่าวสารได้</div>');
        }
    }

    document.getElementById('leftURL').addEventListener('click', function (e) {
        e.preventDefault(); // ป้องกันการเปิดลิงก์
        const url = this.href;
        navigator.clipboard.writeText(url)
            .then(() => {
                alert("คัดลอกลิงก์เรียบร้อยแล้ว!");
            })
            .catch(err => {
                alert("ไม่สามารถคัดลอกลิงก์ได้: " + err);
            });
    });

    document.getElementById('rightURL').addEventListener('click', function (e) {
        e.preventDefault(); // ป้องกันการเปิดลิงก์
        const url = this.href;
        navigator.clipboard.writeText(url)
            .then(() => {
                alert("คัดลอกลิงก์เรียบร้อยแล้ว!");
            })
            .catch(err => {
                alert("ไม่สามารถคัดลอกลิงก์ได้: " + err);
            });
    });

    // เตรียมข้อความทั้งหมดจากข้อมูล
    function prepareMessages(data) {
        allMessages = [];
        const textMaxLength = 150; // กำหนดความยาวสูงสุดของข้อความ

        data.slice(0, 10).forEach((item, index) => {
            // เพิ่มข้อความแต่ละประเภท
            if (item.LastMonthStatusMessage) {
                allMessages.push({
                    type: 'สถานะเดือนที่แล้ว',
                    message: truncateText(item.LastMonthStatusMessage, textMaxLength),
                    originalIndex: index
                });
            }
            if (item.CurrentMonthStatusMessage) {
                allMessages.push({
                    type: 'สถานะเดือนนี้',
                    message: truncateText(item.CurrentMonthStatusMessage, textMaxLength),
                    originalIndex: index
                });
            }
            if (item.SendIDcardMessage) {
                allMessages.push({
                    type: 'ข้อความบัตรประชาชน',
                    message: truncateText(item.SendIDcardMessage, textMaxLength),
                    originalIndex: index
                });
            }
            if (item.SendBankMessage) {
                allMessages.push({
                    type: 'ข้อความธนาคาร',
                    message: truncateText(item.SendBankMessage, textMaxLength),
                    originalIndex: index
                });
            }
            if (item.SendKYCMessage) {
                allMessages.push({
                    type: 'ข้อความ KYC',
                    message: truncateText(item.SendKYCMessage, textMaxLength),
                    originalIndex: index
                });
            }
            if (item.SendHoldExpireMessage) {
                allMessages.push({
                    type: 'ข้อความหมดอายุการระงับ',
                    message: truncateText(item.SendHoldExpireMessage, textMaxLength),
                    originalIndex: index
                });
            }
        });

        return allMessages;
    }

    // แสดงข้อความใน Modal
    function showMessage(index) {
        if (allMessages.length === 0) {
            //alert('ไม่มีข้อความที่จะแสดง');
            return;
        }

        currentMessageIndex = index;
        const message = allMessages[currentMessageIndex];

        // อัปเดตเนื้อหา
        document.getElementById('messageTypeLabel').textContent = message.type;
        document.getElementById('messageContent').textContent = message.message;
        document.getElementById('messageCounter').textContent = `${currentMessageIndex + 1} / ${allMessages.length}`;

        // จัดการปุ่มนำทาง
        document.getElementById('prevBtn').disabled = currentMessageIndex === 0;
        document.getElementById('nextBtn').disabled = currentMessageIndex === allMessages.length - 1;

        // เปลี่ยนข้อความปุ่มถัดไปเป็น "ปิด" ถ้าเป็นข้อความสุดท้าย
        if (currentMessageIndex === allMessages.length - 1) {
            document.getElementById('nextBtn').textContent = 'ปิด';
            document.getElementById('nextBtn').onclick = function () {
                messageModal.hide();
            };
        } else {
            document.getElementById('nextBtn').textContent = 'ถัดไป';
            document.getElementById('nextBtn').onclick = showNextMessage;
        }
    }

    // แสดงข้อความถัดไป
    window.showNextMessage = function () {
        if (currentMessageIndex < allMessages.length - 1) {
            showMessage(currentMessageIndex + 1);
        }
    }

    // แสดงข้อความก่อนหน้า
    window.showPreviousMessage = function () {
        if (currentMessageIndex > 0) {
            showMessage(currentMessageIndex - 1);
        }
    }

    // โหลดข้อมูลและแสดง Modal
    async function loadAndShowMessages() {
        try {
            const response = await fetch('/home/GetMemberMessages', {
                method: 'GET',
                credentials: 'include'
            });
            const data = await response.json();

            if (data && data.length > 0) {
                prepareMessages(data);

                if (allMessages.length > 0) {
                    // เริ่มต้น Modal
                    if (!messageModal) {
                        messageModal = new bootstrap.Modal(document.getElementById('messageModal'));
                    }

                    // แสดงข้อความแรก
                    showMessage(0);
                    messageModal.show();
                } else {
                    //alert('ไม่พบข้อความที่ต้องแสดง');
                }
            } else {
                //alert('ไม่มีข้อมูลจาก API');
            }
        } catch (error) {
            //console.error('Error loading messages:', error);
            //alert('เกิดข้อผิดพลาดในการโหลดข้อความ');
        }
    }

    (async () => {
        if (window.kycStatus !== 'N') {
            await loadAndShowMessages();
        }
        await loadMemberInfo();
        await loadMemberTeamTotalPositionPackage();
        await loadMemberTeamTotalPositionRanking();
        await loadMemberTeamByRegion();
        await loadMemberTeamByRegionBuy();
        await loadMemberTeamBuyProduct();
        await loadMemberTeamNewBuyProduct();
        await loadMemberTeamNewRegister();
        await loadincomeByPeriod();
        await loadBusinessNews();
        await loadMemberEstimatePosition();
    })();
});

function formatNumberWithComma(value) {
    return Number(value).toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}
